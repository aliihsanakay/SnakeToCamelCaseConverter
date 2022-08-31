using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace VSIXProject1
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            
            
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var selection = docView?.TextView.Selection.SelectedSpans.FirstOrDefault();
            if (selection.HasValue)
            {
                var newText = GenerateNewClass(selection.Value.GetText());
                if (newText != null)
                {
                    var concatText = $"{selection.Value.GetText()} \n {newText}";
                    docView.TextBuffer.Replace(selection.Value, concatText);
                }
            }
        }
        private string GenerateNewClass(string text)
        {
            StringBuilder newClass = new StringBuilder();

            try
            {
                var splitText = text.Split('\n');
                var propAspect = "";
                bool addProp = false;
                foreach (var item in splitText)
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        var prop = Regex.Replace(item.Trim(), @"\s+", "");
                        if (prop[0] == '[')
                        {
                            propAspect = prop;
                            addProp = true;
                            continue;
                        }
                        var clearGetSet = prop.Replace("{get;set;}", "").Replace("DateTime", "datetime");
                        var lasSpace = clearGetSet.IndexOfAny(UpperCaseChars);
                        var propName = clearGetSet.Substring(lasSpace, clearGetSet.Length - lasSpace);
                        var propType = clearGetSet.Substring(0, lasSpace).Replace("public", "");

                        var yeniProp = "";
                        var isFirst = true;
                        foreach (var propItem in propName)
                        {
                            if (isFirst)
                                yeniProp += propItem;
                            else
                            {
                                if (propItem == '_')
                                {
                                    isFirst = true;
                                    continue;
                                }
                                else
                                {
                                    yeniProp += propItem.ToString().ToLower(new CultureInfo("en-EN", false));
                                }
                            }

                            isFirst = false;
                        }
                        propType = propType.Replace("datetime", "DateTime").Replace("datetime?", "DateTime?");
                        var newSnakeCaseProp = (addProp ? $"{propAspect}\n" : "") + "public " + propType + " " + yeniProp + " {get;set;}";
                        newClass.AppendLine(newSnakeCaseProp);
                        addProp = false;


                    }
                }
                return newClass.ToString();
            }
            catch (Exception ex)
            {

                VS.MessageBox.ShowError("Hay Aksi !!!", "Sadece Class içini kopyalamalısın \n"+ex.Message);
                return null;
            }
            

        }
        char[] UpperCaseChars = new char[]
           {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H',
                'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
                'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
                'Y', 'Z'
           };
    }
}
