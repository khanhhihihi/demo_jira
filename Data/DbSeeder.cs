using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VietNhatHospital.Models;

namespace VietNhatHospital.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "Admin", "Doctor", "Patient" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Users
        var adminEmail = "admin@vietnhathospital.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = "a1b2c3d4-e5f6-7a8b-9c0d-111111111111",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Hệ thống Admin",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        var doctorEmail = "doctor@vietnhathospital.com";
        var doctorUser = await userManager.FindByEmailAsync(doctorEmail);
        if (doctorUser == null)
        {
            doctorUser = new ApplicationUser
            {
                Id = "b2c3d4e5-f6a7-8b9c-0d1e-222222222222",
                UserName = doctorEmail,
                Email = doctorEmail,
                EmailConfirmed = true,
                FullName = "Bác sĩ Nguyễn Văn A",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(doctorUser, "Doctor@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(doctorUser, "Doctor");
            }
        }

        // Seed Patient user with profile details
        var patientEmail = "patient@vietnhathospital.com";
        var patientUser = await userManager.FindByEmailAsync(patientEmail);
        if (patientUser == null)
        {
            patientUser = new ApplicationUser
            {
                Id = "c3d4e5f6-a7b8-9c0d-1e2f-333333333333",
                UserName = patientEmail,
                Email = patientEmail,
                EmailConfirmed = true,
                FullName = "Bệnh nhân Trần Thị B",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                Height = 170.0, // in cm
                Weight = 65.0,  // in kg
                BirthDate = new DateTime(1995, 5, 15),
                Gender = "female",
                Notes = "Bệnh nhân có tiền sử đau dạ dày nhẹ."
            };
            var result = await userManager.CreateAsync(patientUser, "Patient@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(patientUser, "Patient");
            }
        }

        // 3. Clear and re-seed if Lisinopril is missing or categories are in English
        var needsUpdate = !await context.Conditions.AnyAsync(c => c.Category == "Tiêu hóa");
        if (needsUpdate)
        {
            // Delete dependent records first
            context.PatientConditions.RemoveRange(context.PatientConditions);
            context.DrugContraindications.RemoveRange(context.DrugContraindications);
            context.DrugInteractions.RemoveRange(context.DrugInteractions);
            context.SearchHistories.RemoveRange(context.SearchHistories);
            context.ReviewItems.RemoveRange(context.ReviewItems);
            context.ErrorReports.RemoveRange(context.ErrorReports);
            context.Drugs.RemoveRange(context.Drugs);
            context.Conditions.RemoveRange(context.Conditions);
            await context.SaveChangesAsync();

            // Try to reseed identities for SQL Server
            try
            {
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Drugs', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Conditions', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('DrugContraindications', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('DrugInteractions', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('SearchHistories', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ReviewItems', RESEED, 0)");
                await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ErrorReports', RESEED, 0)");
            }
            catch { }
        }

        // Seed Drugs if empty
        if (!await context.Drugs.AnyAsync())
        {
            var drugs = new List<Drug>
            {
                new Drug { Name = "Paracetamol (Acetaminophen)", ActiveIngredient = "Paracetamol 500mg", DrugGroup = "analgesic", Description = "Thuốc giảm đau, hạ sốt thông thường.", SideEffects = "Tổn thương gan nếu dùng quá liều hoặc dùng lâu ngày." },
                new Drug { Name = "Ibuprofen", ActiveIngredient = "Ibuprofen 400mg", DrugGroup = "analgesic", Description = "Thuốc kháng viêm không steroid (NSAID), giảm đau, kháng viêm.", SideEffects = "Kích ứng dạ dày, nguy cơ gây viêm loét dạ dày tá tràng." },
                new Drug { Name = "Aspirin (Acetylsalicylic Acid)", ActiveIngredient = "Aspirin 81mg", DrugGroup = "analgesic", Description = "Thuốc kháng viêm, hạ sốt, giảm đau và chống kết tập tiểu cầu.", SideEffects = "Tăng nguy cơ chảy máu, viêm loét dạ dày." },
                new Drug { Name = "Metformin", ActiveIngredient = "Metformin Hydrochloride 850mg", DrugGroup = "antidiabetic", Description = "Thuốc điều trị đái tháo đường type 2.", SideEffects = "Nhiễm toan lactic, buồn nôn, tiêu chảy." },
                new Drug { Name = "Insulin Glargine", ActiveIngredient = "Insulin Glargine 100 IU/ml", DrugGroup = "antidiabetic", Description = "Insulin tác dụng kéo dài kiểm soát đường huyết cả ngày.", SideEffects = "Hạ đường huyết đột ngột, phản ứng tại chỗ tiêm." },
                new Drug { Name = "Amoxicillin", ActiveIngredient = "Amoxicillin 500mg", DrugGroup = "antibiotic", Description = "Kháng sinh nhóm penicillin điều trị nhiễm trùng vi khuẩn.", SideEffects = "Tiêu chảy, dị ứng phát ban da." },
                new Drug { Name = "Lisinopril", ActiveIngredient = "Lisinopril 10mg", DrugGroup = "antihypertensive", Description = "Thuốc ức chế men chuyển (ACE) dùng trong điều trị tăng huyết áp và suy tim.", SideEffects = "Ho khan kéo dài, chóng mặt, tăng kali huyết, phù mạch." },
                new Drug { Name = "Amlodipine", ActiveIngredient = "Amlodipine 5mg", DrugGroup = "antihypertensive", Description = "Thuốc chẹn kênh calci nhóm dihydropyridine điều trị tăng huyết áp và thiếu máu cơ tim.", SideEffects = "Phù cổ chân/mắt cá chân, nhức đầu, mặt đỏ bừng." },
                new Drug { Name = "Metoprolol", ActiveIngredient = "Metoprolol Succinate 50mg", DrugGroup = "antihypertensive", Description = "Thuốc chẹn beta chọn lọc beta-1 điều trị cao huyết áp, đau thắt ngực và suy tim mạn.", SideEffects = "Mệt mỏi, nhịp tim chậm, tay chân lạnh, co thắt phế quản nhẹ." },
                new Drug { Name = "Colchicine", ActiveIngredient = "Colchicine 1mg", DrugGroup = "analgesic", Description = "Thuốc kháng viêm chuyên biệt điều trị và dự phòng cơn gút (gout) cấp tính.", SideEffects = "Tiêu chảy nặng, nôn mửa, đau bụng, độc tính cơ nếu dùng dài ngày." },
                new Drug { Name = "Allopurinol", ActiveIngredient = "Allopurinol 300mg", DrugGroup = "analgesic", Description = "Thuốc giảm acid uric máu điều trị bệnh gút mạn tính và phòng sỏi thận urat.", SideEffects = "Phát ban da dị ứng, hội chứng Stevens-Johnson cực kỳ nguy hiểm, sốt." }
            };
            await context.Drugs.AddRangeAsync(drugs);
            await context.SaveChangesAsync();
        }

        // Seed Conditions if empty
        if (!await context.Conditions.AnyAsync())
        {
            var conditions = new List<Condition>
            {
                new Condition { Name = "Suy gan cấp và mãn tính", Category = "Tiêu hóa", IcdCode = "K72", Description = "Chức năng gan suy giảm nặng, giảm khả năng chuyển hóa và giải độc thuốc." },
                new Condition { Name = "Viêm loét dạ dày tá tràng", Category = "Tiêu hóa", IcdCode = "K25", Description = "Tổn thương viêm hoặc loét niêm mạc dạ dày tá tràng, có nguy cơ chảy máu." },
                new Condition { Name = "Suy thận mãn tính", Category = "Thần kinh", IcdCode = "N18", Description = "Chức năng lọc cầu thận giảm dần, giảm đào thải nhiều loại thuốc qua đường tiết niệu." },
                new Condition { Name = "Đái tháo đường Type 2", Category = "Nội tiết", IcdCode = "E11", Description = "Rối loạn chuyển hóa đường huyết do đề kháng insulin hoặc giảm tiết insulin." },
                new Condition { Name = "Hen phế quản", Category = "Hô hấp", IcdCode = "J45", Description = "Tình trạng viêm mạn tính đường thở gây co thắt phế quản, khó thở, ho." },
                new Condition { Name = "Tăng huyết áp vô căn (Cao huyết áp)", Category = "Tim mạch", IcdCode = "I10", Description = "Tình trạng áp lực máu lên thành động mạch tăng cao kéo dài cần kiểm soát." },
                new Condition { Name = "Hẹp động mạch thận hai bên", Category = "Tim mạch", IcdCode = "I70.1", Description = "Hẹp động mạch cung cấp máu cho cả hai bên thận, chống chỉ định dùng thuốc nhóm ACEi/ARB." },
                new Condition { Name = "Bệnh Gút (Gout)", Category = "Nội tiết", IcdCode = "M10", Description = "Rối loạn chuyển hóa acid uric gây viêm khớp cấp và mạn tính." },
                new Condition { Name = "Nhịp tim chậm", Category = "Tim mạch", IcdCode = "R00.1", Description = "Nhịp tim dưới 60 lần/phút, cần tránh các thuốc gây chậm nhịp." },
                new Condition { Name = "Phụ nữ mang thai", Category = "Nội tiết", IcdCode = "Z33", Description = "Trạng thái thai nghén ở phụ nữ, cần hết sức thận trọng vì thuốc có thể qua nhau thai gây hại." }
            };
            await context.Conditions.AddRangeAsync(conditions);
            await context.SaveChangesAsync();
        }

        // Seed DrugContraindications if empty
        if (!await context.DrugContraindications.AnyAsync())
        {
            var paracetamol = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Paracetamol"));
            var ibuprofen = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Ibuprofen"));
            var aspirin = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Aspirin"));
            var metformin = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Metformin"));
            var lisinopril = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Lisinopril"));
            var metoprolol = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Metoprolol"));
            var colchicine = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Colchicine"));

            var suyGan = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Suy gan"));
            var loetDaDay = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Viêm loét dạ dày"));
            var suyThan = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Suy thận"));
            var hepDongMach = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Hẹp động mạch"));
            var mangThai = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("mang thai"));
            var henPheQuan = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Hen phế quản"));
            var nhipCham = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Nhịp tim chậm"));

            var contraindications = new List<DrugContraindication>();

            if (paracetamol != null && suyGan != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = paracetamol.DrugId,
                    ConditionId = suyGan.ConditionId,
                    Severity = "critical",
                    Reason = "Paracetamol chuyển hóa chủ yếu qua gan. Suy gan làm tăng nguy cơ ngộ độc gan nặng và hoại tử tế bào gan.",
                    Alternative = "Không dùng hoặc giảm liều tối đa và tham khảo ý kiến bác sĩ."
                });
            }

            if (ibuprofen != null && loetDaDay != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = ibuprofen.DrugId,
                    ConditionId = loetDaDay.ConditionId,
                    Severity = "critical",
                    Reason = "Ibuprofen ức chế COX-1 làm giảm tổng hợp prostaglandin bảo vệ niêm mạc dạ dày, tăng nguy cơ chảy máu dạ dày.",
                    Alternative = "Paracetamol (Acetaminophen) để giảm đau nhẹ, hoặc các chất ức chế chọn lọc COX-2 kèm PPI."
                });
            }

            if (aspirin != null && loetDaDay != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = aspirin.DrugId,
                    ConditionId = loetDaDay.ConditionId,
                    Severity = "critical",
                    Reason = "Aspirin gây kích ứng trực tiếp niêm mạc dạ dày và chống kết tập tiểu cầu, cực kỳ nguy hiểm cho người viêm loét đang tiến triển.",
                    Alternative = "Các biện pháp thay thế tùy thuộc vào mục đích sử dụng (giảm đau hay chống đông máu)."
                });
            }

            if (metformin != null && suyThan != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = metformin.DrugId,
                    ConditionId = suyThan.ConditionId,
                    Severity = "warning",
                    Reason = "Metformin thải trừ qua thận. Suy thận gây tích lũy metformin dẫn đến nhiễm toan lactic đe dọa tính mạng.",
                    Alternative = "Insulin hoặc các nhóm thuốc trị tiểu đường khác không thải trừ qua thận."
                });
            }

            if (lisinopril != null && hepDongMach != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = lisinopril.DrugId,
                    ConditionId = hepDongMach.ConditionId,
                    Severity = "critical",
                    Reason = "Thuốc ức chế men chuyển làm giãn tiểu động mạch đi ở cầu thận, làm giảm mạnh áp lực lọc cầu thận ở bệnh nhân hẹp động mạch thận cả hai bên, gây suy thận cấp tính.",
                    Alternative = "Thuốc chẹn kênh calci (như Amlodipine) hoặc lợi tiểu khác."
                });
            }

            if (lisinopril != null && mangThai != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = lisinopril.DrugId,
                    ConditionId = mangThai.ConditionId,
                    Severity = "critical",
                    Reason = "Thuốc ức chế men chuyển (ACE) gây độc tính trực tiếp lên thai nhi ở quý 2 và 3, dẫn đến vô niệu, thiểu ối, dị tật bẩm sinh hoặc chết lưu.",
                    Alternative = "Dùng Methyldopa hoặc Labetalol để kiểm soát huyết áp thai kỳ."
                });
            }

            if (metoprolol != null && henPheQuan != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = metoprolol.DrugId,
                    ConditionId = henPheQuan.ConditionId,
                    Severity = "critical",
                    Reason = "Metoprolol là thuốc chẹn beta. Dù chọn lọc beta-1, ở liều điều trị vẫn có thể chẹn thụ thể beta-2 ở phế quản, gây co thắt phế quản dữ dội khởi phát cơn hen cấp.",
                    Alternative = "Sử dụng các nhóm hạ áp khác như chẹn kênh calci DHP (Amlodipine) hoặc ức chế men chuyển."
                });
            }

            if (metoprolol != null && nhipCham != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = metoprolol.DrugId,
                    ConditionId = nhipCham.ConditionId,
                    Severity = "critical",
                    Reason = "Chẹn beta làm giảm dẫn truyền nút nhĩ thất và giảm nhịp xoang, khiến tình trạng nhịp tim chậm nghiêm trọng hơn, có nguy cơ gây ngừng tim.",
                    Alternative = "Các nhóm hạ áp khác không ảnh hưởng nhịp tim như Lisinopril hoặc Amlodipine."
                });
            }

            if (colchicine != null && suyThan != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = colchicine.DrugId,
                    ConditionId = suyThan.ConditionId,
                    Severity = "warning",
                    Reason = "Colchicine đào thải qua thận và mật. Suy thận mãn tính làm tích lũy thuốc, tăng nguy cơ độc tính thần kinh, cơ, rụng tóc và suy tủy xương.",
                    Alternative = "Sử dụng Corticosteroid đường uống ngắn ngày hoặc tiêm nội khớp."
                });
            }

            if (colchicine != null && suyGan != null)
            {
                contraindications.Add(new DrugContraindication
                {
                    DrugId = colchicine.DrugId,
                    ConditionId = suyGan.ConditionId,
                    Severity = "warning",
                    Reason = "Giảm chuyển hóa colchicine qua gan làm tăng độc tính toàn thân nghiêm trọng.",
                    Alternative = "Cân nhắc sử dụng Corticosteroid hoặc các biện pháp giảm đau tại chỗ."
                });
            }

            await context.DrugContraindications.AddRangeAsync(contraindications);
            await context.SaveChangesAsync();
        }

        // 6. Seed Drug Interactions if empty
        if (!await context.DrugInteractions.AnyAsync())
        {
            var lisinopril = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Lisinopril"));
            var ibuprofen = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Ibuprofen"));
            var aspirin = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Aspirin"));
            var metoprolol = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Metoprolol"));
            var amlodipine = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Amlodipine"));
            var allopurinol = await context.Drugs.FirstOrDefaultAsync(d => d.Name.Contains("Allopurinol"));

            if (ibuprofen != null && aspirin != null)
            {
                await context.DrugInteractions.AddAsync(new DrugInteraction
                {
                    DrugId1 = ibuprofen.DrugId,
                    DrugId2 = aspirin.DrugId,
                    Level = "warning",
                    Description = "Sử dụng đồng thời Ibuprofen và Aspirin làm tăng đáng kể nguy cơ xuất huyết dạ dày và làm giảm tác dụng bảo vệ tim mạch của Aspirin liều thấp.",
                    Recommendation = "Cần tránh sử dụng chung. Uống cách xa nhau ít nhất 8 tiếng."
                });
            }

            if (lisinopril != null && ibuprofen != null)
            {
                await context.DrugInteractions.AddAsync(new DrugInteraction
                {
                    DrugId1 = lisinopril.DrugId,
                    DrugId2 = ibuprofen.DrugId,
                    Level = "warning",
                    Description = "NSAID (như Ibuprofen) làm giảm hiệu quả hạ huyết áp của Lisinopril và tăng nguy cơ suy thận cấp, đặc biệt ở bệnh nhân cao tuổi hoặc mất nước.",
                    Recommendation = "Hạn chế phối hợp. Theo dõi huyết áp và chức năng thận thường xuyên nếu phải sử dụng đồng thời."
                });
            }

            if (metoprolol != null && amlodipine != null)
            {
                await context.DrugInteractions.AddAsync(new DrugInteraction
                {
                    DrugId1 = metoprolol.DrugId,
                    DrugId2 = amlodipine.DrugId,
                    Level = "warning",
                    Description = "Phối hợp thuốc chẹn beta và chẹn kênh calci có thể gây tác dụng cộng lực, làm hạ huyết áp quá mức hoặc gây chậm nhịp tim nghiêm trọng.",
                    Recommendation = "Theo dõi nhịp tim và huyết áp của bệnh nhân thường xuyên."
                });
            }

            if (aspirin != null && allopurinol != null)
            {
                await context.DrugInteractions.AddAsync(new DrugInteraction
                {
                    DrugId1 = aspirin.DrugId,
                    DrugId2 = allopurinol.DrugId,
                    Level = "warning",
                    Description = "Aspirin liều thấp làm giảm thải acid uric ở ống thận, đối kháng trực tiếp và làm giảm tác dụng điều trị hạ acid uric của Allopurinol.",
                    Recommendation = "Tránh dùng Aspirin liều giảm đau cho người bệnh gout đang dùng Allopurinol. Cân nhắc thay thế bằng Paracetamol."
                });
            }
            await context.SaveChangesAsync();
        }

        // 7. Seed PatientConditions (Link patient Trần Thị B to "Viêm loét dạ dày tá tràng" and "Phụ nữ mang thai")
        if (patientUser != null)
        {
            var loetDaDay = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("Viêm loét dạ dày"));
            if (loetDaDay != null)
            {
                var exists = await context.PatientConditions.AnyAsync(pc => pc.UserId == patientUser.Id && pc.ConditionId == loetDaDay.ConditionId);
                if (!exists)
                {
                    await context.PatientConditions.AddAsync(new PatientCondition
                    {
                        UserId = patientUser.Id,
                        ConditionId = loetDaDay.ConditionId
                    });
                }
            }

            var mangThai = await context.Conditions.FirstOrDefaultAsync(c => c.Name.Contains("mang thai"));
            if (mangThai != null)
            {
                var exists = await context.PatientConditions.AnyAsync(pc => pc.UserId == patientUser.Id && pc.ConditionId == mangThai.ConditionId);
                if (!exists)
                {
                    await context.PatientConditions.AddAsync(new PatientCondition
                    {
                        UserId = patientUser.Id,
                        ConditionId = mangThai.ConditionId
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // 8. Seed SearchHistories
        if (!await context.SearchHistories.AnyAsync())
        {
            var histories = new List<SearchHistoryItem>
            {
                new SearchHistoryItem { Timestamp = DateTime.UtcNow.AddDays(-1), Type = "Thuốc", Keyword = "Paracetamol", Result = "Tìm thấy 1 kết quả (Suy gan cấp và mãn tính)", DetailsUrl = "/drugs/1", UserId = patientUser?.Id },
                new SearchHistoryItem { Timestamp = DateTime.UtcNow.AddHours(-12), Type = "Tương tác", Keyword = "Ibuprofen, Aspirin", Result = "Cảnh báo mức độ Warning (Tăng nguy cơ chảy máu dạ dày)", DetailsUrl = "", UserId = patientUser?.Id },
                new SearchHistoryItem { Timestamp = DateTime.UtcNow.AddHours(-8), Type = "Hồ sơ", Keyword = "Cập nhật bệnh nền", Result = "Thêm Viêm loét dạ dày tá tràng", DetailsUrl = "", UserId = patientUser?.Id },
                new SearchHistoryItem { Timestamp = DateTime.UtcNow.AddHours(-2), Type = "Thuốc", Keyword = "Metformin", Result = "Tìm thấy 1 kết quả (Suy thận mãn tính)", DetailsUrl = "/drugs/4", UserId = patientUser?.Id },
                new SearchHistoryItem { Timestamp = DateTime.UtcNow.AddMinutes(-10), Type = "Thuốc", Keyword = "Amoxicillin", Result = "Tìm thấy 1 kết quả (Safe)", DetailsUrl = "/drugs/6", UserId = patientUser?.Id }
            };
            await context.SearchHistories.AddRangeAsync(histories);
            await context.SaveChangesAsync();
        }

        // 9. Seed ReviewItems
        if (!await context.ReviewItems.AnyAsync())
        {
            var reviews = new List<ReviewItem>
            {
                new ReviewItem { Code = "MB13-001", Type = "Thêm thuốc mới", Content = "Thêm thuốc giảm đau thế hệ mới Meloxicam", Status = "pending", Reviewer = "Hệ thống tự động", Reference = "Dược thư quốc gia 2022" },
                new ReviewItem { Code = "MB13-002", Type = "Sửa chống chỉ định", Content = "Cập nhật chống chỉ định Paracetamol với người suy gan nặng", Status = "approved", Reviewer = "Bác sĩ Nguyễn Văn A", Reference = "Quyết định Bộ Y Tế" },
                new ReviewItem { Code = "MB13-003", Type = "Thêm tương tác", Content = "Thêm tương tác thuốc Clopidogrel và Omeprazole", Status = "rejected", Reviewer = "Hệ thống tự động", Reference = "FDA Warning 2020", RejectionNote = "Thiếu tài liệu chứng minh lâm sàng chi tiết." },
                new ReviewItem { Code = "MB13-004", Type = "Sửa thông tin thuốc", Content = "Chỉnh sửa liều dùng tối đa của Ibuprofen trong ngày", Status = "needsRevision", Reviewer = "Bác sĩ Trần Văn B", Reference = "MIMS Việt Nam", RejectionNote = "Cần làm rõ liều dùng cho trẻ em dưới 12 tuổi." }
            };
            await context.ReviewItems.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }

        // 10. Seed ErrorReports
        if (!await context.ErrorReports.AnyAsync())
        {
            var reports = new List<ErrorReport>
            {
                new ErrorReport { Code = "MB12-001", DrugName = "Paracetamol", ErrorType = "Sai thông tin chống chỉ định", Reporter = "Bác sĩ Nguyễn Văn A", Role = "Doctor", Status = "pending", Priority = "high", Description = "Thông tin chống chỉ định cho Paracetamol với suy gan chưa đủ chi tiết về liều giới hạn." },
                new ErrorReport { Code = "MB12-002", DrugName = "Ibuprofen", ErrorType = "Sai tương tác thuốc", Reporter = "Bệnh nhân Trần Thị B", Role = "Patient", Status = "inReview", Priority = "medium", Description = "Tương tác giữa Ibuprofen và Aspirin ghi là Warning nhưng thực tế lâm sàng cần xếp vào Critical." },
                new ErrorReport { Code = "MB12-003", DrugName = "Metformin", ErrorType = "Thiếu thông tin thay thế", Reporter = "Dược sĩ Phạm Minh C", Role = "Doctor", Status = "resolved", Priority = "low", Description = "Nên bổ sung thêm Insulin Glargine làm thuốc thay thế đề xuất cho Metformin khi suy thận.", AdminNote = "Đã bổ sung Insulin Glargine vào danh sách thay thế đề xuất." }
            };
            await context.ErrorReports.AddRangeAsync(reports);
            await context.SaveChangesAsync();
        }
    }
}
