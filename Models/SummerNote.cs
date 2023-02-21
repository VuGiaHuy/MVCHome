namespace App.Models;
public class SummerNote 
{
    public SummerNote(string idEditor, bool load = true)
    {
        IdEditor = idEditor;
        Load = load;
    }
    public string IdEditor {get;set;}
    public bool Load {get;set;}
    public int tabsize {get;set;}= 2;
    public int height {get;set;}= 120;
    public string toolbar  {get;set;} = @"
        [
            ['style', ['bold', 'italic', 'underline', 'clear']],
            ['font', ['strikethrough', 'superscript', 'subscript']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'video']],
            ['view', ['fullscreen', 'codeview', 'help']]
        ]
    ";
}