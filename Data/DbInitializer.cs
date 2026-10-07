using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Models;

namespace RaceDayAPI.Data;

public static class DbInitializer
{
    public static void Seed(RaceDayDbContext db)
    {
        // Only seed when there are no users yet (avoids duplicates on restart)
        if (db.Users.Any())
            return;

        var hasher = new PasswordHasher<User>();

        // ---------------- Users (matches SQL seed data) ----------------
        var thandiwe = new User
        {
            RoleID = 1,
            FullName = "Thandiwe Mokoena",
            Email = "thandiwe@raceday.co.za",
            PhoneNumber = "0821234567"
        };
        var james = new User
        {
            RoleID = 1,
            FullName = "James Whitfield",
            Email = "james@raceday.co.za",
            PhoneNumber = "0837654321"
        };
        var tina = new User
        {
            RoleID = 2,
            FullName = "Tina Jeke",
            Email = "tina@example.com",
            PhoneNumber = "0845551122"
        };
        var tshepo = new User
        {
            RoleID = 2,
            FullName = "Tshepo Tau",
            Email = "tshepo@example.com",
            PhoneNumber = "0845552233"
        };
        var amy = new User
        {
            RoleID = 2,
            FullName = "Amy Richards",
            Email = "amy@example.com",
            PhoneNumber = "0845553344"
        };

        // Seed passwords are hashed properly so these accounts CAN log in.
        // Password for every seeded account: "Password123!"
        thandiwe.PasswordHash = hasher.HashPassword(thandiwe, "Password123!");
        james.PasswordHash = hasher.HashPassword(james, "Password123!");
        tina.PasswordHash = hasher.HashPassword(tina, "Password123!");
        tshepo.PasswordHash = hasher.HashPassword(tshepo, "Password123!");
        amy.PasswordHash = hasher.HashPassword(amy, "Password123!");

        db.Users.AddRange(thandiwe, james, tina, tshepo, amy);
        db.SaveChanges();

        // ---------------- Events ----------------
        var joburgRun = new Event
        {
            OrganiserID = thandiwe.UserID,
            EventName = "Joburg City Run",
            EventDate = new DateTime(2026, 10, 11),
            Location = "Johannesburg, Gauteng",
            Description = "Annual road race through the Johannesburg CBD."
        };
        var sowetoMarathon = new Event
        {
            OrganiserID = thandiwe.UserID,
            EventName = "Soweto Marathon",
            EventDate = new DateTime(2026, 11, 15),
            Location = "Soweto, Gauteng",
            Description = "Marathon and half-marathon through historic Soweto."
        };
        var capeFunRun = new Event
        {
            OrganiserID = james.UserID,
            EventName = "Cape Coastal Fun Run",
            EventDate = new DateTime(2026, 9, 27),
            Location = "Cape Town, Western Cape",
            Description = "Family-friendly coastal fun run and 10km race."
        };

        db.Events.AddRange(joburgRun, sowetoMarathon, capeFunRun);
        db.SaveChanges();

        // ---------------- Categories ----------------
        var funRun5km = new Category
        {
            EventID = joburgRun.EventID,
            CategoryName = "5km Fun Run",
            DistanceKm = 5.00m,
            MaxParticipants = 500,
            EntryFee = 100.00m
        };
        var race10km = new Category
        {
            EventID = joburgRun.EventID,
            CategoryName = "10km Race",
            DistanceKm = 10.00m,
            MaxParticipants = 300,
            EntryFee = 150.00m
        };
        var halfMarathon = new Category
        {
            EventID = sowetoMarathon.EventID,
            CategoryName = "Half Marathon 21km",
            DistanceKm = 21.10m,
            MaxParticipants = 200,
            EntryFee = 250.00m
        };
        var fullMarathon = new Category
        {
            EventID = sowetoMarathon.EventID,
            CategoryName = "Full Marathon 42km",
            DistanceKm = 42.20m,
            MaxParticipants = 150,
            EntryFee = 350.00m
        };
        var familyFunRun = new Category
        {
            EventID = capeFunRun.EventID,
            CategoryName = "2km Family Fun Run",
            DistanceKm = 2.00m,
            MaxParticipants = 400,
            EntryFee = 50.00m
        };
        var coastalRace = new Category
        {
            EventID = capeFunRun.EventID,
            CategoryName = "10km Coastal Race",
            DistanceKm = 10.00m,
            MaxParticipants = 250,
            EntryFee = 150.00m
        };

        db.Categories.AddRange(funRun5km, race10km, halfMarathon,
                               fullMarathon, familyFunRun, coastalRace);
        db.SaveChanges();

        // ---------------- Enrolments ----------------
        var e1 = new Enrolment { UserID = tina.UserID, CategoryID = funRun5km.CategoryID, Status = "Confirmed" };
        var e2 = new Enrolment { UserID = tina.UserID, CategoryID = halfMarathon.CategoryID, Status = "Confirmed" };
        var e3 = new Enrolment { UserID = tshepo.UserID, CategoryID = race10km.CategoryID, Status = "Confirmed" };
        var e4 = new Enrolment { UserID = tshepo.UserID, CategoryID = coastalRace.CategoryID, Status = "Registered" };
        var e5 = new Enrolment { UserID = amy.UserID, CategoryID = familyFunRun.CategoryID, Status = "Confirmed" };

        db.Enrolments.AddRange(e1, e2, e3, e4, e5);
        db.SaveChanges();

        // ---------------- Results ----------------
        db.Results.AddRange(
            new Result
            {
                EnrolmentID = e1.EnrolmentID,
                FinishTime = TimeSpan.Parse("00:24:35"),
                Position = 1,
                Status = "Finished"
            },
            new Result
            {
                EnrolmentID = e3.EnrolmentID,
                FinishTime = TimeSpan.Parse("00:52:10"),
                Position = 1,
                Status = "Finished"
            }
        );
        db.SaveChanges();
    }
}
