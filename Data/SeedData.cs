using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RoktoLink.Models;
using RoktoLink.Models.Enums;

namespace RoktoLink.Data
{
    /// <summary>
    /// Seeds the SQLite database with initial roles, admin, coordinator, and BMDC-verified doctor accounts,
    /// 100 authentic Dhaka donors, real-world clinical requisitions, medical screenings, and historical donations.
    /// </summary>
    public static class SeedData
    {
        private const string AdminPasswordHash = "AQAAAAEAAYagAAAAENb9MmguMtSu1bG/5izZ3ljYKvBuEkS6cCIKYLpWuP5f2qbLjFoaPMiJ83uRnDy2rA==";
        private const string StandardDonorHash = "AQAAAAIAAYagAAAAEJ4+6OVUUA2faxrjK9GynyHDQdd/TLl1LjSvZl7SFHNFaQiBY26/5hWgi1H29YNPGA==";
        private const string StandardSecurityStamp = "HV67ELZS35HGVCB5V33QZAQ3BX4J4CBI";

        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            await context.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS CoordinatorProfiles (Id INTEGER PRIMARY KEY AUTOINCREMENT, UserId TEXT NOT NULL, HospitalName TEXT NOT NULL, Department TEXT NOT NULL, StaffId TEXT NULL, IsApproved INTEGER NOT NULL, ApprovedAt TEXT NULL, RegisteredAt TEXT NOT NULL, FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE);");

            string[] roles = { "Admin", "Coordinator", "Donor", "Doctor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var donorRole = await roleManager.FindByNameAsync("Donor");
            var donorRoleId = donorRole?.Id ?? Guid.NewGuid().ToString();

            var adminUser = await userManager.FindByEmailAsync("beyondsafa@gmail.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "beyondsafa@gmail.com",
                    Email = "beyondsafa@gmail.com",
                    FullName = "System Admin (Safa)",
                    District = "Dhaka",
                    EmailConfirmed = true,
                    PasswordHash = AdminPasswordHash,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                var result = await userManager.CreateAsync(adminUser);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                adminUser.PasswordHash = AdminPasswordHash;
                adminUser.SecurityStamp = Guid.NewGuid().ToString();
                await userManager.UpdateAsync(adminUser);
            }

            var coordinatorLogins = new[]
            {
                ("coordinator@roktolink.local", "Hospital Coordinator", "Coord@1234", "General Hospital Dhaka", "Transfusion Coordinator"),
                ("dmch.coordinator@roktolink.local", "DMCH Blood Bank Coordinator", "Coord@1234", "Dhaka Medical College Hospital", "Senior Blood Bank Coordinator"),
                ("bsmmu.coordinator@roktolink.local", "BSMMU Transfusion Coordinator", "Coord@1234", "Bangabandhu Sheikh Mujib Medical University", "Transfusion Medicine Specialist"),
                ("quantum.coordinator@roktolink.local", "Quantum Lab Coordinator", "Coord@1234", "Quantum Lab Blood Bank", "Laboratory Coordinator")
            };

            foreach (var (email, fullName, password, hospital, dept) in coordinatorLogins)
            {
                var coordinator = await userManager.FindByEmailAsync(email);
                if (coordinator == null)
                {
                    coordinator = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        District = "Dhaka",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(coordinator, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(coordinator, "Coordinator");
                    }
                }

                if (!await context.CoordinatorProfiles.AnyAsync(c => c.UserId == coordinator.Id))
                {
                    context.CoordinatorProfiles.Add(new CoordinatorProfile
                    {
                        UserId = coordinator.Id,
                        HospitalName = hospital,
                        Department = dept,
                        StaffId = "STAFF-" + coordinator.Id.Substring(0, Math.Min(6, coordinator.Id.Length)).ToUpper(),
                        IsApproved = true,
                        ApprovedAt = DateTime.UtcNow.AddDays(-30)
                    });
                    await context.SaveChangesAsync();
                }
            }

            var doctorLogins = new[]
            {
                ("dr.tanvir@roktolink.local", "Dr. Tanvir Ahmed", "A-89214", "Dhaka Medical College Hospital", "Assistant Professor, Transfusion Medicine", true, "Doctor@1234"),
                ("dr.sadia@roktolink.local", "Dr. Sadia Islam", "A-94102", "Bangabandhu Sheikh Mujib Medical University (BSMMU)", "Medical Officer, Clinical Hematology", false, "Doctor@1234")
            };

            foreach (var (email, fullName, bmdc, college, designation, isVerified, password) in doctorLogins)
            {
                var docUser = await userManager.FindByEmailAsync(email);
                if (docUser == null)
                {
                    docUser = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        District = "Shahbagh, Dhaka",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(docUser, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(docUser, "Doctor");

                        context.DoctorProfiles.Add(new DoctorProfile
                        {
                            UserId = docUser.Id,
                            BMDCRegistrationNumber = bmdc,
                            MedicalCollege = college,
                            Designation = designation,
                            IsBMDCVerified = isVerified,
                            VerifiedAt = isVerified ? DateTime.UtcNow.AddDays(-30) : null
                        });
                        await context.SaveChangesAsync();
                    }
                }
            }

            if (await userManager.FindByEmailAsync("donor@roktolink.local") == null)
            {
                var testDonor = new ApplicationUser
                {
                    UserName = "donor@roktolink.local",
                    Email = "donor@roktolink.local",
                    FullName = "Hero Donor",
                    District = "Dhanmondi, Dhaka",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(testDonor, "Donor@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testDonor, "Donor");
                }
            }

            if (context.DonorProfiles.Count() < 50)
            {
                context.MedicalScreenings.RemoveRange(context.MedicalScreenings);
                context.DonationRecords.RemoveRange(context.DonationRecords);
                context.BloodRequests.RemoveRange(context.BloodRequests);
                context.DonorProfiles.RemoveRange(context.DonorProfiles);

                var oldBatchedUsers = context.Users.Where(u => u.Email != null && u.Email.StartsWith("dhaka.donor")).ToList();
                context.Users.RemoveRange(oldBatchedUsers);
                await context.SaveChangesAsync();

                var dhakaAreas = new[]
                {
                    "Dhanmondi, Dhaka", "Mirpur-1, Dhaka", "Mirpur-10, Dhaka", "Uttara Sector 3, Dhaka",
                    "Uttara Sector 11, Dhaka", "Mohammadpur, Dhaka", "Gulshan-1, Dhaka", "Gulshan-2, Dhaka",
                    "Banani, Dhaka", "Bashundhara R/A, Dhaka", "Badda, Dhaka", "Motijheel, Dhaka",
                    "Khilgaon, Dhaka", "Malibagh, Dhaka", "Shahbagh, Dhaka", "Lalbagh, Dhaka",
                    "Old Dhaka, Dhaka", "Farmgate, Dhaka", "Tejgaon, Dhaka", "Rampura, Dhaka",
                    "Jatrabari, Dhaka", "Mogbazar, Dhaka", "Kakrail, Dhaka", "Shantinagar, Dhaka",
                    "Eskaton, Dhaka", "Rajarbagh, Dhaka"
                };

                var donorNames = new[]
                {
                    "Tanvir Ahmed", "Nabila Rahman", "Fahim Shahriar", "Sadia Islam", "Mehdi Hasan",
                    "Nusrat Jahan", "Shakil Mahmud", "Farzana Yasmin", "Anik Chowdhury", "Sumaiya Akter",
                    "Rifat Hossain", "Tasnim Fatima", "Asif Iqbal", "Jannatul Ferdous", "Zaber Al Mamun",
                    "Mehnaz Haque", "Sabbir Rahman", "Afsana Mimi", "Sourav Das", "Lamia Tabassum",
                    "Touhidul Islam", "Puja Roy", "Mahmudul Hasan", "Shaila Sharmin", "Nahid Parvez",
                    "Nazmun Nahar", "Ariful Islam", "Israt Jahan", "Sajjad Hossain", "Tanjina Sultana",
                    "Shahadat Hossain", "Mitu Akter", "Enamul Haque", "Sabina Yasmin", "Golam Rabbani",
                    "Rashedul Karim", "Moushumi Sen", "Zahidul Islam", "Rumana Afroz", "Ashiqur Rahman",
                    "Farhana Boby", "Mustafizur Rahman", "Sharmin Shila", "Imtiaz Ahmed", "Rehana Begum",
                    "Tariqul Islam", "Sonia Khatun", "Kamrul Hasan", "Nasrin Sultana", "Kazi Muntasir",
                    "Bushra Khan", "Saiful Bari", "Shahnaz Parvin", "Mizanur Rahman", "Tasmia Alam",
                    "Rubel Mia", "Purnima Rani", "Jahid Hasan", "Sabera Sultana", "Shafiqul Alam",
                    "Farzana Rimi", "Mahfuzur Rahman", "Rupa Paul", "Atiqur Rahman", "Munira Siraj",
                    "Al Amin", "Shanta Islam", "Biplob Kumar", "Dilruba Yasmin", "Monirul Islam",
                    "Sanjida Haque", "Emdadul Haque", "Tania Ahmed", "Abu Sayed", "Sharmin Akter",
                    "Harun Rashid", "Fatematuz Zohra", "Nasir Uddin", "Rumki Das", "Joynal Abedin",
                    "Shamima Nasrin", "Babul Hossain", "Laila Noor", "Shamsur Rahman", "Shirin Akter",
                    "Kabir Hossain", "Zannat Ara", "Mozammel Haque", "Poly Saha", "Delwar Hossain",
                    "Afroza Begum", "Sirajul Islam", "Rashida Khatun", "Rezaul Karim", "Salma Akter",
                    "Mokbul Hossain", "Nazma Banu", "Azizul Haque", "Tahmina Chowdhury", "Khairul Anam"
                };

                var bloodTypeDistribution = new List<string>();
                for (int i = 0; i < 34; i++) bloodTypeDistribution.Add("B+");
                for (int i = 0; i < 33; i++) bloodTypeDistribution.Add("O+");
                for (int i = 0; i < 24; i++) bloodTypeDistribution.Add("A+");
                for (int i = 0; i < 5; i++) bloodTypeDistribution.Add("AB+");
                bloodTypeDistribution.Add("O-");
                bloodTypeDistribution.Add("O-");
                bloodTypeDistribution.Add("A-");
                bloodTypeDistribution.Add("B-");

                var newUsers = new List<ApplicationUser>();
                var userRolesToAdd = new List<IdentityUserRole<string>>();
                var newProfiles = new List<DonorProfile>();

                var testDonorUser = await userManager.FindByEmailAsync("donor@roktolink.local");
                if (testDonorUser != null)
                {
                    newProfiles.Add(new DonorProfile
                    {
                        UserId = testDonorUser.Id,
                        BloodType = "O+",
                        District = "Dhanmondi, Dhaka",
                        IsAvailable = true,
                        TotalDonations = 3,
                        LastDonationDate = DateTime.UtcNow.AddDays(-105)
                    });
                }

                for (int i = 0; i < donorNames.Length; i++)
                {
                    var userId = Guid.NewGuid().ToString();
                    var email = $"dhaka.donor{i + 1}@roktolink.local";
                    var area = dhakaAreas[i % dhakaAreas.Length];
                    var bloodType = bloodTypeDistribution[i % bloodTypeDistribution.Count];

                    var user = new ApplicationUser
                    {
                        Id = userId,
                        UserName = email,
                        NormalizedUserName = email.ToUpperInvariant(),
                        Email = email,
                        NormalizedEmail = email.ToUpperInvariant(),
                        FullName = donorNames[i],
                        District = area,
                        EmailConfirmed = true,
                        SecurityStamp = StandardSecurityStamp,
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        PasswordHash = StandardDonorHash
                    };

                    newUsers.Add(user);
                    userRolesToAdd.Add(new IdentityUserRole<string>
                    {
                        UserId = userId,
                        RoleId = donorRoleId
                    });

                    DateTime? lastDonationDate = null;
                    int totalDonations = 0;
                    bool isAvailable = true;

                    if (i % 10 == 0)
                    {
                        isAvailable = false;
                    }
                    else if (i % 3 == 0)
                    {
                        lastDonationDate = DateTime.UtcNow.AddDays(-(15 + (i * 2) % 65));
                        totalDonations = (i % 4) + 1;
                    }
                    else if (i % 2 == 0)
                    {
                        lastDonationDate = DateTime.UtcNow.AddDays(-(95 + (i * 3) % 200));
                        totalDonations = (i % 6) + 1;
                    }

                    newProfiles.Add(new DonorProfile
                    {
                        UserId = userId,
                        BloodType = bloodType,
                        District = area,
                        IsAvailable = isAvailable,
                        TotalDonations = totalDonations,
                        LastDonationDate = lastDonationDate
                    });
                }

                context.Users.AddRange(newUsers);
                context.UserRoles.AddRange(userRolesToAdd);
                context.DonorProfiles.AddRange(newProfiles);
                await context.SaveChangesAsync();

                var activeCoordinator = await userManager.FindByEmailAsync("dmch.coordinator@roktolink.local")
                                     ?? await userManager.FindByEmailAsync("coordinator@roktolink.local");

                if (activeCoordinator != null)
                {
                    var requests = new List<BloodRequest>
                    {
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Begum Rokeya (Gynae Ward)",
                            BloodType = "O-",
                            UnitsNeeded = 2,
                            Hospital = "Dhaka Medical College Hospital (DMCH)",
                            District = "Shahbagh, Dhaka",
                            UrgencyLevel = UrgencyLevel.Critical,
                            Status = RequestStatus.Open,
                            Deadline = DateTime.UtcNow.AddHours(18),
                            Notes = "Emergency Postpartum Hemorrhage (PPH). Immediate O-Negative blood needed.",
                            CreatedAt = DateTime.UtcNow.AddHours(-3)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Master Ayan (Pediatric Hematology)",
                            BloodType = "B+",
                            UnitsNeeded = 2,
                            Hospital = "Bangabandhu Sheikh Mujib Medical University (BSMMU)",
                            District = "Shahbagh, Dhaka",
                            UrgencyLevel = UrgencyLevel.Urgent,
                            Status = RequestStatus.Open,
                            Deadline = DateTime.UtcNow.AddDays(1),
                            Notes = "Thalassemia Major routine monthly blood transfusion exchange.",
                            CreatedAt = DateTime.UtcNow.AddHours(-8)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Md. Rafiqul Islam (Cardiac OT)",
                            BloodType = "A+",
                            UnitsNeeded = 3,
                            Hospital = "National Institute of Cardiovascular Diseases (NICVD)",
                            District = "Sher-e-Bangla Nagar, Dhaka",
                            UrgencyLevel = UrgencyLevel.Critical,
                            Status = RequestStatus.Open,
                            Deadline = DateTime.UtcNow.AddHours(24),
                            Notes = "Open-heart coronary artery bypass graft (CABG) scheduled.",
                            CreatedAt = DateTime.UtcNow.AddHours(-12)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Shirin Sultana (Trauma Unit)",
                            BloodType = "O+",
                            UnitsNeeded = 4,
                            Hospital = "National Institute of Traumatology (NITOR / Pongu Hospital)",
                            District = "Sher-e-Bangla Nagar, Dhaka",
                            UrgencyLevel = UrgencyLevel.Critical,
                            Status = RequestStatus.Open,
                            Deadline = DateTime.UtcNow.AddHours(12),
                            Notes = "Major road traffic accident multiple pelvic fractures. Emergency surgery.",
                            CreatedAt = DateTime.UtcNow.AddHours(-2)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Tariqul Hasan (Medicine Ward)",
                            BloodType = "A-",
                            UnitsNeeded = 1,
                            Hospital = "Kurmitola General Hospital",
                            District = "Dhaka Cantonment, Dhaka",
                            UrgencyLevel = UrgencyLevel.Urgent,
                            Status = RequestStatus.Open,
                            Deadline = DateTime.UtcNow.AddDays(2),
                            Notes = "Dengue hemorrhagic fever with severe acute thrombocytopenia.",
                            CreatedAt = DateTime.UtcNow.AddHours(-16)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Abdur Rahman (Dialysis Unit)",
                            BloodType = "B-",
                            UnitsNeeded = 1,
                            Hospital = "BIRDEM General Hospital",
                            District = "Shahbagh, Dhaka",
                            UrgencyLevel = UrgencyLevel.Normal,
                            Status = RequestStatus.InProgress,
                            Deadline = DateTime.UtcNow.AddDays(3),
                            Notes = "Chronic kidney disease dialysis support. Coordinator awaiting volunteer.",
                            CreatedAt = DateTime.UtcNow.AddDays(-1)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Rubina Akter (Oncology)",
                            BloodType = "AB+",
                            UnitsNeeded = 2,
                            Hospital = "Square Hospital",
                            District = "Panthapath, Dhaka",
                            UrgencyLevel = UrgencyLevel.Urgent,
                            Status = RequestStatus.InProgress,
                            Deadline = DateTime.UtcNow.AddDays(2),
                            Notes = "Acute myeloid leukemia induction chemotherapy support.",
                            CreatedAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new()
                        {
                            CoordinatorId = activeCoordinator.Id,
                            PatientName = "Kazi Aminul Haque",
                            BloodType = "O+",
                            UnitsNeeded = 2,
                            Hospital = "Evercare Hospital Dhaka",
                            District = "Bashundhara R/A, Dhaka",
                            UrgencyLevel = UrgencyLevel.Normal,
                            Status = RequestStatus.Fulfilled,
                            Deadline = DateTime.UtcNow.AddDays(-5),
                            Notes = "Elective hip arthroplasty replacement. Successfully completed.",
                            CreatedAt = DateTime.UtcNow.AddDays(-7),
                            FulfilledAt = DateTime.UtcNow.AddDays(-6)
                        }
                    };

                    context.BloodRequests.AddRange(requests);
                    await context.SaveChangesAsync();

                    var savedProfiles = context.DonorProfiles.ToList();
                    var savedRequests = context.BloodRequests.ToList();
                    var doctorTanvir = await context.DoctorProfiles.FirstOrDefaultAsync(d => d.BMDCRegistrationNumber == "A-89214");

                    if (savedProfiles.Any() && savedRequests.Any())
                    {
                        var req1 = savedRequests[0];
                        var donor1 = savedProfiles.FirstOrDefault(d => d.BloodType == "O-") ?? savedProfiles[0];
                        var donor2 = savedProfiles.FirstOrDefault(d => d.BloodType == "B+") ?? savedProfiles[1];

                        context.MedicalScreenings.AddRange(
                            new MedicalScreening
                            {
                                RequestId = req1.Id,
                                DonorId = donor1.Id,
                                DoctorId = doctorTanvir?.Id,
                                WeightKg = 68.5,
                                HemoglobinLevel = 14.2,
                                SystolicBP = 120,
                                DiastolicBP = 80,
                                HasNoRecentInfection = true,
                                HasNoRecentTattooOrSurgery = true,
                                CooldownConfirmed = true,
                                Status = ScreeningStatus.Approved,
                                DoctorRemarks = "Medically fit. Normal vitals. Cleared for immediate whole blood donation.",
                                CreatedAt = DateTime.UtcNow.AddHours(-2),
                                ScreenedAt = DateTime.UtcNow.AddHours(-1)
                            },
                            new MedicalScreening
                            {
                                RequestId = savedRequests[1].Id,
                                DonorId = donor2.Id,
                                WeightKg = 54.0,
                                HemoglobinLevel = 13.1,
                                SystolicBP = 115,
                                DiastolicBP = 75,
                                HasNoRecentInfection = true,
                                HasNoRecentTattooOrSurgery = true,
                                CooldownConfirmed = true,
                                Status = ScreeningStatus.Pending,
                                CreatedAt = DateTime.UtcNow.AddHours(-1)
                            }
                        );
                        await context.SaveChangesAsync();

                        var records = new List<DonationRecord>();
                        var random = new Random(42);

                        for (int m = 11; m >= 0; m--)
                        {
                            var donationMonth = DateTime.UtcNow.AddMonths(-m);
                            int donationsInMonth = random.Next(2, 6);

                            for (int k = 0; k < donationsInMonth; k++)
                            {
                                var d = savedProfiles[random.Next(savedProfiles.Count)];
                                var r = savedRequests[random.Next(savedRequests.Count)];

                                records.Add(new DonationRecord
                                {
                                    DonorId = d.Id,
                                    RequestId = r.Id,
                                    UnitsGiven = random.Next(1, 3),
                                    DonationDate = donationMonth.AddDays(random.Next(1, 28)),
                                    ConfirmedByCoordinator = true
                                });
                            }
                        }

                        context.DonationRecords.AddRange(records);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
