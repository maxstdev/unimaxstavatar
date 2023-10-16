using Maxst.Passport;

public class SampleConfig : PassportConfig
{
    public override ClientType clientType => ClientType.Public;

    public override string Realm => "maxst";

    public override string ApplicationId => "a95596dc-9671-4104-9c35-b3813ba6485f";

    public override string ApplicationKey => "LaXGvymRETB0itkarTQyPoB8Dym6pU7v";

    public override string GrantType => "client_credentials";

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
