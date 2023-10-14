using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace COL.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    // Database connection and data retrieval
    public List<CityState> CityStates { get; set; }

    public double OldCostIndex { get; private set; }

    public double NewCostIndex { get; private set; }

    public int NewIncome { get; set; }

    public string SelectedNewCity { get; set; }

    public double CostDifference { get; set; }

    public double IncomeTax { get; set; }

    public string HigherOrLower { get; set; }

    public double DisplayDifference { get; set; }

    public void OnGet()
    {
        // Path to the CSV file in the main directory
        string csvFilePath = "col-index.csv";

        // Read all lines from the CSV file
        string[] csvLines = System.IO.File.ReadAllLines(csvFilePath);

        CityStates = new List<CityState>();

        // Assuming the CSV structure is like City,State,CostIndex
        foreach (string line in csvLines.Skip(1)) // Skip the header line if there's one
        {
            string[] parts = line.Split(',');

            if (parts.Length >= 3)
            {
                string city = parts[0].Trim();
                string state = parts[1].Trim();

                if (double.TryParse(parts[2], out double costIndex))
                {
                    CityStates.Add(new CityState { City = city, State = state, CostIndex = costIndex });
                }
                else
                {
                    // Log an error or handle invalid data
                    _logger.LogError($"Invalid CostIndex in line: {line}");
                }
            }
            else
            {
                // Log an error or handle improperly formatted data
                _logger.LogError($"Invalid data in line: {line}");
            }
        }
    }

    public IActionResult OnPostCalculateDifference()
    {
        // Read the values from the form fields
        string selectedOldCity = Request.Form["SelectedOldCity"];
        SelectedNewCity = Request.Form["SelectedNewCity"];

        if (!string.IsNullOrEmpty(selectedOldCity) && !string.IsNullOrEmpty(SelectedNewCity))
        {
            // Get income value from form field
            if (int.TryParse(Request.Form["Income"], out int currIncome))
            {
                // Separate old city and state
                string[] splitOld = selectedOldCity.Split(',');
                string oldCity = splitOld[0].Trim();
                string oldState = splitOld[1].Trim();

                // Separate new city and state
                string[] splitNew = SelectedNewCity.Split(',');
                string newCity = splitNew[0].Trim();
                string newState = splitNew[1].Trim();

                // Path to the CSV file in the main directory
                string csvFilePath = "col-index.csv";

                // Read all lines from the CSV file
                string[] csvLines = System.IO.File.ReadAllLines(csvFilePath);

                // Find cost indices from the CSV file
                double? oldCostIndex = FindCostIndex(csvLines, oldCity, oldState);
                double? newCostIndex = FindCostIndex(csvLines, newCity, newState);

                if (oldCostIndex != null && newCostIndex != null)
                {
                    // Handle nullable values appropriately (using GetValueOrDefault())
                    CostDifference = newCostIndex.GetValueOrDefault() / oldCostIndex.GetValueOrDefault();

                    // Calculate cost difference to display on screen
                    DisplayDifference = Math.Truncate((CostDifference - 1) * 100);

                    // Calculate new income in the new city
                    NewIncome = Convert.ToInt32(CostDifference * currIncome);

                    // Calculate whether the new cost of living is higher or lower
                    HigherOrLower = (DisplayDifference > 0) ? "higher" : "lower";

                    // Get rid of the negative sign so it doesn't appear on the screen
                    if (DisplayDifference < 0)
                        DisplayDifference *= -1;
                }
                else
                {
                    // Handle the case where cost indices are not found
                    _logger.LogError("Cost indices not found for selected cities");
                }
            }
            else
            {
                // Log an error if the 'Income' value is not valid
                _logger.LogError("Income is not a valid integer");
            }
        }
        else
        {
            // Handle the case where selected cities are null or empty
            _logger.LogError("Selected cities are null or empty");
        }

        // Return the same page with updated CostDifference value
        return new JsonResult(new { SelectedNewCity, NewIncome, DisplayDifference, HigherOrLower });
    }

    private double? FindCostIndex(string[] csvLines, string city, string state)
    {
        // Assuming the CSV structure is like City,State,CostIndex
        foreach (string line in csvLines.Skip(1)) // Skip the header line if there's one
        {
            string[] parts = line.Split(',');

            if (parts.Length >= 3)
            {
                string csvCity = parts[0].Trim();
                string csvState = parts[1].Trim();

                if (csvCity == city && csvState == state && double.TryParse(parts[2], out double costIndex))
                {
                    return costIndex;
                }
            }
        }

        return null; // Cost index not found
    }
}