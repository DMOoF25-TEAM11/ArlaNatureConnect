namespace ArlaNatureConnect.WinUI.ViewModels.Constants;

/// <summary>
/// Shared constants for Person-related viewmodels.
/// </summary>
public static class ViewModelConstants
{
    // Page Titles


    // Common Labels
    public static string ADDRESS => Translate("Adresse");
    public static string ADDRESS_CITY => Translate("By");
    public static string ADDRESS_COUNTRY => Translate("Land");
    public static string ADDRESS_POSTAL_CODE => Translate("Postnummer");
    public static string ADDRESS_STREET => Translate("Vejnavn og nummer");
    public static string EMAIL => Translate("Email");
    public static string FARM_NAME => Translate("Gårdens Navn");
    public static string FARMS_COUNT => Translate("Antal Gårde");
    public static string IS_ACTIVE => Translate("Aktiv");
    public static string NATURE_AREA_DETAILS => Translate("Naturareal Detaljer");
    public static string NATURE_AREA_NAME => Translate("Naturareal Navn");
    public static string PERSON_FIRSTNAME => Translate("Fornavn");
    public static string PERSON_LASTNAME => Translate("Efternavn");
    public static string PERSON_ROLE => Translate("Rolle");
    public static string SELECT_A_FARM => Translate("Vælg en gård");
    public static string SELECT_FARM => Translate("Vælg Gård");
    public static string NATURE_AREA_DESCRIPTION => Translate("Naturareal Beskrivelse");
    public static string LONGITUDE => Translate("Længdegrad");
    public static string LATITUDE => Translate("Breddegrad");

    private static string Translate(string text)
    {
        // Implement translation logic here
        return text;
    }
}
