using Microsoft.EntityFrameworkCore;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Services.Interfaces;
using Sym_Connect__Capstone2_.ViewModels.ActivityHistoryViewModel;

namespace Sym_Connect__Capstone2_.Services.Implementation
{
    public class ActivityHistoryService : IActivityHistoryService
    {
        private readonly AppDbContext _context;

        public ActivityHistoryService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<ActivityHistoryViewModel> GetActivityHistoryAsync(int memberId, string userId)
        {
            var activities =
                new List<ActivityHistoryItemViewModel>();

            // ==========================================
            // 1. EVENT HISTORY
            // ==========================================

            var eventHistory =
                await _context.Attendances
                    .AsNoTracking()
                    .Where(a =>
                        a.MemberId == memberId &&
                        (
                            a.Status == "Present" ||
                            a.Status == "Late" ||
                            a.Status == "Absent"
                        ))
                    .Select(a =>
                        new ActivityHistoryItemViewModel
                        {
                            Id = a.EventId,

                            ActivityType = "Event",

                            Title =
                                a.Event.EventTitle,

                            Date =
                                a.Event.StartDate,

                            StartTime =
                                a.Event.StartTime,

                            EndTime =
                                a.Event.EndTime,

                            Location =
                                a.Event.ExpectedFacilities
                                    .Select(ef =>
                                        ef.Facility.FacilityName)
                                    .FirstOrDefault()
                                ?? "No facility",

                            Status =
                                a.Status,

                            Points =
                                a.Points
                        })
                    .ToListAsync();


            activities.AddRange(eventHistory);


            // ==========================================
            // 2. ELECTION HISTORY
            // ==========================================

            var electionHistory =
                await _context.Votes
                    .AsNoTracking()

                    // Only votes made by this user
                    .Where(v =>
                        v.UserId == userId)

                    // One election can contain many votes
                    // because the user votes for different
                    // positions.
                    //
                    // We only want ONE history row
                    // for the whole election.
                    .GroupBy(v =>
                        v.ElectionId)

                    .Select(g =>
                        new ActivityHistoryItemViewModel
                        {
                            Id =
                                g.Key,

                            ActivityType =
                                "Election",

                            Title =
                                g.First()
                                    .Election
                                    .ElectionName,

                            Date =
                                g.First()
                                    .Election
                                    .StartDate,

                            StartTime =
                                g.First()
                                    .Election
                                    .StartTime,

                            EndTime =
                                g.First()
                                    .Election
                                    .EndTime,

                            Location =
                                "Online Voting",

                            Status =
                                "Voted",

                            Points = 0
                        })
                    .ToListAsync();


            activities.AddRange(electionHistory);


            // ==========================================
            // 3. EVALUATION HISTORY
            // ==========================================

            var evaluationHistory =
                await _context.EvaluationSubmissions
                    .AsNoTracking()

                    .Where(s =>
                        s.UserId == userId &&
                        s.Status ==
                            SubmissionStatus.Submitted && s.SubmittedAt.HasValue)

                    .Select(s =>
                        new ActivityHistoryItemViewModel
                        {
                            Id =
                                s.EvaluationId,

                            ActivityType =
                                "Evaluation",

                            Title =
                                s.Evaluation
                                    .EvaluationTitle,

                            // Use the actual date
                            // the member submitted it.
                            Date =
                                s.SubmittedAt.Value,

                            StartTime = null,

                            EndTime = null,

                            Location =
                                s.Evaluation.Event != null
                                    ? s.Evaluation.Event
                                        .EventTitle
                                    : "No event",

                            Status =
                                "Submitted",

                            Points = 0
                        })
                    .ToListAsync();


            activities.AddRange(evaluationHistory);


            // ==========================================
            // 4. NEWEST ACTIVITY FIRST
            // ==========================================

            activities =
                activities
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.StartTime)
                    .ToList();


            // ==========================================
            // 5. RETURN VIEW MODEL
            // ==========================================

            return new ActivityHistoryViewModel
            {
                Activities = activities
            };
        }
    }
}
    

