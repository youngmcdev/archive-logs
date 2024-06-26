namespace mcy.Tools.Models;


public class CreateOptionRequestBool: BaseCreateOptionRequest{}

public class BaseCreateOptionRequest
{
    public string Name {get;set;} = String.Empty;
    public string Description {get;set;} = String.Empty;
    public string Alias {get;set;} = String.Empty;
}