using System;
using System.Globalization;

namespace EstimateCalculator
{
    class Program
    {
        private static double workingTimeInHours;
        private static double plannedRandomizationInHours;
        private static double unplannedRandomizationHours;
        private static double featureEstimateInDays;
        private static DateTime startD, endD;
        private static string pattern = "MM/dd/yy";

        // The following are calculated
        private static double featureEstimateInHours, perDayAllocationHours, daysToCompleteProject;


        static void Main(string[] args)
        {
            Console.WriteLine("Hello! \nEnter 1 for estimating time for your feature. \nEnter 2 for estimating resources for your feature with a deadline.");
            var choice = int.Parse(Console.ReadLine());
            switch(choice)
            {
                case 1:
                    EstimateTime();
                    return;
                case 2:
                    EstimateResources();
                    return;
            }

            Console.WriteLine("Press any key to exit!");
            Console.ReadKey();
            return;
        }

        private static void FetchInputAndCalculateBasicValues()
        {
            Console.WriteLine("Enter working time in hours per day: (double)");
            workingTimeInHours = ValidateDoubleInput(Console.ReadLine());

            Console.WriteLine("Enter planned randomization in hours per day (meetings + PR Reviews etc.): (double)");
            plannedRandomizationInHours = ValidateDoubleInput(Console.ReadLine());

            Console.WriteLine("Enter unplanned randomization time in hours per day: (double)");
            unplannedRandomizationHours = ValidateDoubleInput(Console.ReadLine());

            Console.WriteLine("Enter feature estimate in days: (double)");
            featureEstimateInDays = ValidateDoubleInput(Console.ReadLine());

            Console.WriteLine($"Enter the startdate of your feature in {pattern} format: Eg.: 05/30/20");
            var startDateString = Console.ReadLine();
            if (!DateTime.TryParseExact(startDateString, pattern, null, DateTimeStyles.None, out startD))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Date is not in correct format. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            perDayAllocationHours = PerDayAllocation(
                workingTimeInHours,
                plannedRandomizationInHours,
                unplannedRandomizationHours);

            featureEstimateInHours = featureEstimateInDays * workingTimeInHours;

            daysToCompleteProject = RoundDoubleToNearestPointFive(featureEstimateInHours / perDayAllocationHours);
        }

        private static void EstimateTime()
        {
            FetchInputAndCalculateBasicValues(); 
            endD = GetEndDateBasedOnBusinessCalendar(startD, daysToCompleteProject);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"On any given day, you can allocate {perDayAllocationHours} hours in your feature.");
            Console.WriteLine($"It will take {daysToCompleteProject} days to finish the feature.");
            Console.WriteLine($"You will finish the feature on {endD.ToString(pattern)}.");            
        }

        private static void EstimateResources()
        {
            FetchInputAndCalculateBasicValues();

            Console.WriteLine($"Enter the endDate of your feature in {pattern} format: Eg.: 05/30/20");
            var endDateString = Console.ReadLine();
            if (!DateTime.TryParseExact(endDateString, pattern, null, DateTimeStyles.None, out endD))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Date is not in correct format. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            var businessDaysAvailable = GetBusinessDaysBetweenStartAndEnd(startD, endD);
            var resources = RoundDoubleToNearestPointFive(daysToCompleteProject / businessDaysAvailable);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"On any given day, you can allocate {perDayAllocationHours} hours in your feature.");
            Console.WriteLine($"It will take {daysToCompleteProject} days to finish the feature.");
            Console.WriteLine($"So, to complete it by {endD.ToString(pattern)}, you will need approximately {resources} resources to finish the feature.");
        }

        private static double ValidateDoubleInput(string input)
        {
            double output = 0;
            if(!double.TryParse(input, out output) || output <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Input needs to be a valid double > 0. Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            return output;
        }

        private static double PerDayAllocation(
            double workingTimeInHours,
            double plannedRandomizationInHours,
            double unplannedRandomizationHours)
        {
            var actualTimeToWorkOnProject = workingTimeInHours - plannedRandomizationInHours;
            var actualTimeExcludingRandomizations = actualTimeToWorkOnProject - unplannedRandomizationHours;

            return actualTimeExcludingRandomizations;
        }

        private static DateTime GetEndDateBasedOnBusinessCalendar(DateTime startTime, double daysToCompleteProject)
        {
            daysToCompleteProject = Math.Ceiling(daysToCompleteProject);
            DateTime endDate = new DateTime(startTime.Year, startTime.Month, startTime.Day);

            while(daysToCompleteProject > 0)
            {
                if(endDate.DayOfWeek != DayOfWeek.Saturday && endDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    --daysToCompleteProject;
                }

                endDate = endDate.AddDays(1);
            }

            return endDate.AddDays(-1);
        }

        private static double GetBusinessDaysBetweenStartAndEnd(DateTime startD, DateTime endD)
        {
            var days = (endD - startD).TotalDays + 1;
            for(var i = startD; i <= endD; i = i.AddDays(1))
            {
                if(i.DayOfWeek == DayOfWeek.Saturday || i.DayOfWeek == DayOfWeek.Sunday)
                {
                    days--;
                }
            }

            return days;
        }

        public static double RoundDoubleToNearestPointFive(double input)
        {
            input = Math.Round(input, 1);
            var floor = Math.Floor(input);
            if (input - floor <= 0.1)
                return floor;

            if (input - floor <= 0.6)
                return floor + 0.5;

            return floor + 1;
        }
    }
}
