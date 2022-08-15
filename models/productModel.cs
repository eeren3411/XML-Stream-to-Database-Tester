using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace models;
[BsonIgnoreExtraElements]
public class productModel{
    public productModel(
        string sku,
        string name,
        string url,
        string imgUrl,
        string description,
        bool distributor,
        float price,
        float shipPrice,
        int quantity,
        string productBrand,
        string productCategory,
        string barcode
    ){
        this.sku = sku;
        this.name = name;
        this.url = url;
        this.imgUrl = imgUrl;
        this.description = description;
        this.distributor = distributor;
        this.price = price;
        this.shipPrice = shipPrice;
        this.quantity = quantity;
        this.productBrand = productBrand;
        this.productCategory = productCategory;
        this.barcode = barcode;
    }
    public string sku {get; set;}
    public string name{get; set;}
    public string url{get; set;}
    public string imgUrl{get; set;}
    public string description{get; set;}
    public bool distributor{get; set;}
    public float price{get; set;}
    public float shipPrice{get; set;}
    public int quantity{get; set;}
    public string productBrand{get; set;}
    public string productCategory{get; set;}
    public string barcode{get; set;}

    public override string ToString()
    {
        return string.Join("\n",
        $"sku -> {this.sku}",
        $"name -> {this.name}",
        $"url -> {this.url}",
        $"imgUrl -> {this.imgUrl}",
        $"description -> {this.description}",
        $"distributor -> {this.distributor}",
        $"price -> {this.price}",
        $"shipPrice -> {this.shipPrice}",
        $"quantity -> {this.quantity}",
        $"productBrand -> {this.productBrand}",
        $"productCategory -> {this.productCategory}",
        $"barcode -> {this.barcode}\n");
    }
}