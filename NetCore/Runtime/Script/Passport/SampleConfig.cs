using Maxst.Passport;

public class SampleConfig : PassportConfig
{
    public override ClientType clientType => ClientType.Public;

    public override string Realm => "";

    public override string ApplicationId => "";

    public override string ApplicationKey => "";

    public override string GrantType => "";

    private static SampleConfig instance;
    public static SampleConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SampleConfig();

            }
            return instance;
        }
    }
}
