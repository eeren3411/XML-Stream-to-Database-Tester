using System.Xml;
using System.Globalization;

namespace Tester;

static class ExtensionClass{
    public static IEnumerable<models.productModel> products(this XmlReader source){
        Dictionary<string, string> productData = new Dictionary<string,string>();
        string sourceName = "";
        while(source.Read()){
            //Console.WriteLine($"source name here -> {source.Name} | source value here -> {source.Value}");
            if (source.NodeType == XmlNodeType.Element && source.Name == "product"){
                //Console.WriteLine("clear dictionary");
                productData.Clear();
            }
            else if (source.NodeType == XmlNodeType.EndElement && source.Name == "product"){
                //Console.WriteLine("create product");
                var temp="";
                yield return new models.productModel(
                    productData.TryGetValue("sku", out temp)? temp: "missing",
                    productData.TryGetValue("name", out temp)? temp: "missing",
                    productData.TryGetValue("url", out temp)? temp: "missing",
                    productData.TryGetValue("imgUrl", out temp)? temp: "missing",
                    productData.TryGetValue("description", out temp)? temp: "missing",
                    bool.Parse(productData.TryGetValue("distributor", out temp)? temp: "false"),
                    float.Parse(productData.TryGetValue("price", out temp)? temp: "0.0", CultureInfo.InvariantCulture),
                    float.Parse(productData.TryGetValue("shipPrice", out temp)? temp: "0.0", CultureInfo.InvariantCulture),
                    int.Parse(productData.TryGetValue("quantity", out temp)? temp: "0"),
                    productData.TryGetValue("productBrand", out temp)? temp: "missing",
                    productData.TryGetValue("productCategory", out temp)? temp: "missing",
                    productData.TryGetValue("barcode", out temp)? temp: "missing"
                );
            }
            else if (source.NodeType == XmlNodeType.Element){
                //Console.WriteLine("save source name");
                sourceName = source.Name;
            }
            else if (source.NodeType == XmlNodeType.Text || source.NodeType == XmlNodeType.CDATA){
                //Console.WriteLine($"add to productdata which is {sourceName}, {source.Value}");
                //productData.Add(sourceName, source.Value); it was causing problems in some corrupted xml files
                productData[sourceName] = source.Value; //this works even in case of product endelement object is missing
            }
        }
    }
}