namespace mcy.Tools.Models;


public class CreateBoolCliOptionRequest: BaseCreateCliOptionRequest{}

public class BaseCreateCliOptionRequest
{
    public string Name {get;set;} = String.Empty;
    public string Description {get;set;} = String.Empty;
    public string Alias {get;set;} = String.Empty;
}