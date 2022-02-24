using System.Collections.Immutable;

var Promotions = ImmutableArray.Create(new Promotions[] { new Discount { Priority = 2 }, new BottlePriceDrop(0001) });

ProductFactory wBottle = new(0001, "Watter Bottle", 24.95M);
ProductFactory hoodie = new(0002, "Hoodie", 65.00M);
ProductFactory stickerSet = new(0003, "Sticker Set ", 3.99M);

//App starts:
//1.
//Test DATA: Items: 0001,0001,0002,0003
//Expected test price = 103.47
Console.WriteLine("Items: 0001,0001,0002,0003");
Checkout checkout1 = new(Promotions);
List<Product> products = new()
{
    wBottle.CreateProduct(),
    wBottle.CreateProduct(),
    hoodie.CreateProduct(),
    stickerSet.CreateProduct()
};
checkout1.Scan(products);
Console.WriteLine($"{checkout1.Total()}");

//2.
//Test DATA: Items: 0001,0001,0001
//Expected test price = 68.97
Console.WriteLine("Items: 0001,0001,0001");
Checkout checkout2 = new(Promotions);
List<Product> products2 = new()
{
    wBottle.CreateProduct(),
    wBottle.CreateProduct(),
    wBottle.CreateProduct()
};
checkout2.Scan(products2);
Console.WriteLine($"{checkout2.Total()}");

//3.
//Test DATA: Items: 0002,0002,0003
//Expected test price = 120.59
Console.WriteLine("Items: 0002,0002,0003");
Checkout checkout3 = new(Promotions);
List<Product> products3 = new()
{
    hoodie.CreateProduct(),
    hoodie.CreateProduct(),
    stickerSet.CreateProduct()
};
checkout3.Scan(products3);
Console.WriteLine($"{checkout3.Total()}");

record Product(decimal Id, string Name = "")
{
    public decimal Price { get; set; }
    public override string ToString() => $"ID: {Id:0000}, Name: {Name}, Price: {Price}";
}
record ProductFactory(decimal id, string name, decimal price)
{
    public Product CreateProduct() => new(id, name) { Price = price };
}
abstract record Promotions(int Priority = 1)
{
    public long CreateDate { get; init; } = DateTime.Now.Ticks;
    public abstract void Apply(List<Product> products);
}
record Checkout(ImmutableArray<Promotions> promotions)
{
    List<Product> Products { get; set; } = new();
    public void Scan(List<Product> products) => Products = products;
    public string Total()
    {
        promotions.OrderBy(p => p.Priority).ThenBy(p => p.CreateDate).ToList().ForEach(p => p.Apply(Products));
        return $"Total Price: £{Products.Select(x => x.Price).Sum().ToString("0.##")}";
    }
}
//If you spend over £75 then you get a 10% discount
record Discount(int priceToApply = 75, decimal discountPercentage = 10) : Promotions
{
    public override void Apply(List<Product> products)
    {
        if (products.Select(x => x.Price).Sum() > priceToApply)
            products.ForEach(x => x.Price -= x.Price * 0.01M * discountPercentage);
    }
}
//If you buy two or more water bottles then the price drops to £22.99 each
record BottlePriceDrop(decimal waterBottleId, int wBottCount = 2, decimal newWaterPrice = 22.99M) : Promotions
{
    public override void Apply(List<Product> products)
    {
        var wBott = products.Where(x => x.Id == waterBottleId).ToList();
        if (wBott.Count >= wBottCount)
            wBott.ForEach(x => x.Price = newWaterPrice);
    }
}