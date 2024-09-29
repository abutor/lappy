using System.ComponentModel.DataAnnotations;

namespace Lappy.Core.Models;

public class TableRequest
{
    public int Page { get; set; } = 1;
    
    public int PageSize { get; set; } = 25;
    
    public string OrderBy { get; set; } = string.Empty;
    
    [AllowedValues("ask", "desk")]
    public bool Direction { get; set; }
}
