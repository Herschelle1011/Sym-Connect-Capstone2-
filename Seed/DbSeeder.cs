using CloudinaryDotNet.Actions;
using Sym_Connect__Capstone2_.Data;
using Sym_Connect__Capstone2_.Models.Enums;
using Sym_Connect__Capstone2_.Models.FacilityModels;

namespace Sym_Connect__Capstone2_.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            //IF FACILITIES ALREADY EXIST RETURNS
            if (context.Facilities.Any()) return;

            var facilities = new List<Facility>
            {
                new Facility
                {
                    FacilityName = "Main Gym",
                    FacilityDescription = "A full-size indoor court with stage access, ideal for basketball, events, and community gatherings throughout the year.",
                    Status = FacilityStatus.Active,
                    FacilityTypes = "Basketball,OpenCourt",
                    Capacity = 120,

                        FacilityPhotos = new List<FacilityPhoto>{
                        new FacilityPhoto
                        {
                            PhotoUrl = "https://res.cloudinary.com/dxzmz62w4/image/upload/v1780411501/gym_tbewcy.png"
                        },
                        new FacilityPhoto
                        {
                            PhotoUrl = "https://res.cloudinary.com/dxzmz62w4/image/upload/v1780411705/gym2_jlrosv.webp"
                        }
                    }
                },

                 new Facility
                {
                    FacilityName = "Study Hall",
                    FacilityDescription = "A full-size indoor court with stage access, ideal for basketball, events, and community gatherings throughout the year.",
                    Status = FacilityStatus.Active,
                    FacilityTypes = "Study,MeetingRoom",
                    Capacity = 120,

                        FacilityPhotos = new List<FacilityPhoto>{
                        new FacilityPhoto
                        {
                            //CONTROLLER NEXT ALREADY DONE 
                            PhotoUrl = "https://res.cloudinary.com/dxzmz62w4/image/upload/v1780411845/basketball_slmbmh.png"
                        },
                    }
                },

                 new Facility
{
    FacilityName = "Youth Activity Hall",
    FacilityDescription = "A spacious hall used for youth gatherings, workshops, seminars, and indoor activities.",
    Status = FacilityStatus.Active,
    FacilityTypes = "EventHall,YouthActivities",
    Capacity = 150,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865"
        }
    }
},

new Facility
{
    FacilityName = "Conference Room",
    FacilityDescription = "A professional meeting space equipped for planning sessions, leadership meetings, and presentations.",
    Status = FacilityStatus.Active,
    FacilityTypes = "MeetingRoom,Conference",
    Capacity = 40,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72"
        }
    }
},

new Facility
{
    FacilityName = "Youth Library",
    FacilityDescription = "A quiet study area with books, resources, and spaces for research and personal learning.",
    Status = FacilityStatus.Active,
    FacilityTypes = "Library,Study",
    Capacity = 50,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1507842217343-583bb7270b66"
        }
    }
},

new Facility
{
    FacilityName = "Music Practice Room",
    FacilityDescription = "A dedicated room for choir rehearsals, band practice, and musical training sessions.",
    Status = FacilityStatus.Active,
    FacilityTypes = "Music,PracticeRoom",
    Capacity = 25,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1511379938547-c1f69419868d"
        }
    }
},

new Facility
{
    FacilityName = "Computer Laboratory",
    FacilityDescription = "A technology-focused room equipped with computers for training, workshops, and digital learning.",
    Status = FacilityStatus.Active,
    FacilityTypes = "ComputerLab,Training",
    Capacity = 35,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3"
        }
    }
},

new Facility
{
    FacilityName = "Prayer Chapel",
    FacilityDescription = "A peaceful worship area for prayer services, reflection, and spiritual activities.",
    Status = FacilityStatus.Active,
    FacilityTypes = "Chapel,Worship",
    Capacity = 80,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1438232992991-995b7058bbb3"
        }
    }
},

new Facility
{
    FacilityName = "Outdoor Pavilion",
    FacilityDescription = "An open-air venue suitable for community events, team-building activities, and social gatherings.",
    Status = FacilityStatus.Active,
    FacilityTypes = "Outdoor,EventSpace",
    Capacity = 100,

    FacilityPhotos = new List<FacilityPhoto>
    {
        new FacilityPhoto
        {
            PhotoUrl = "https://images.unsplash.com/photo-1517457373958-b7bdd4587205"
        }
    }
}
            };
            await context.Facilities.AddRangeAsync(facilities);
            await context.SaveChangesAsync();

        
            
        }
    }
}
