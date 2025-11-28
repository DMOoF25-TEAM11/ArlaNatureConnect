namespace ArlaNatureConnect.WinUI.Helpers;

/// <summary>
/// Provides translation methods for priority values between English (database) and Danish (UI display).
/// </summary>
public static class PriorityTranslator
{
    /// <summary>
    /// Converts English priority value (from database) to Danish (for UI display).
    /// </summary>
    /// <param name="englishPriority">The English priority value (e.g., "Low", "Medium", "High", "Urgent").</param>
    /// <returns>The Danish equivalent (e.g., "Lav", "Normal", "Høj", "Haster"), or the original value if no translation is found.</returns>
    public static string? ToDanish(string? englishPriority)
    {
        return englishPriority switch
        {
            "Low" => "Lav",
            "Medium" => "Normal",
            "High" => "Høj",
            "Urgent" => "Haster",
            _ => englishPriority
        };
    }

    /// <summary>
    /// Converts Danish priority value (from UI) to English (for database storage).
    /// </summary>
    /// <param name="danishPriority">The Danish priority value (e.g., "Lav", "Normal", "Høj", "Haster").</param>
    /// <returns>The English equivalent (e.g., "Low", "Medium", "High", "Urgent"), or the original value if no translation is found.</returns>
    public static string? ToEnglish(string? danishPriority)
    {
        return danishPriority switch
        {
            "Lav" => "Low",
            "Normal" => "Medium",
            "Høj" => "High",
            "Haster" => "Urgent",
            _ => danishPriority
        };
    }
}

