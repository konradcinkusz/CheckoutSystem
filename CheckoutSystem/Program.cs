//Set this flag to true if you want to show logs
bool enableLogs = false;

List<Promotions> Promotions = new() { new Discount(75, 10) { Priority = 2 }, new BottlePriceDrop(0001, 2, (decimal)22.99) };

ProductFactory wBottle = new(0001, "Watter Bottle", (decimal)24.95);
ProductFactory hoodie = new(0002, "Hoodie", (decimal)65.00);
ProductFactory stickerSet = new(0003, "Sticker Set ", (decimal)3.99);

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
if (enableLogs)
{
    Console.WriteLine();
    Console.WriteLine("Logs:");
    Globals.Logs.ForEach(log => Console.WriteLine(log));
    Globals.ClearLogs();
    Console.WriteLine();
}


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
if (enableLogs)
{
    Console.WriteLine();
    Console.WriteLine("Logs:");
    Globals.Logs.ForEach(log => Console.WriteLine(log));
    Globals.ClearLogs();
}

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
if (enableLogs)
{
    Console.WriteLine();
    Console.WriteLine("Logs:");
    Globals.Logs.ForEach(log => Console.WriteLine(log));
    Globals.ClearLogs();
}

static class Globals
{
    public static char Currency = '£';
    public static List<string> Logs { get; private set; } = new List<string>();
    public static void ClearLogs() => Logs = new List<string>();
}

class Product
{
    public decimal Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public override string ToString()
        => $"ID: {Id:0000}, Name: {Name}, Price: {Price}";
}
class ProductFactory
{
    private readonly decimal id;
    private readonly string name;
    private readonly decimal price;

    public ProductFactory(decimal id, string name, decimal price)
    {
        this.id = id;
        this.name = name;
        this.price = price;
    }
    public Product CreateProduct() => new() { Id = id, Name = name, Price = price };
}
abstract class Promotions
{
    public int Priority { get; set; } = 1;
    public long CreateDate { get; } = DateTime.Now.Ticks;
    public abstract void Apply(List<Product> products);
}

class Checkout
{
    private readonly List<Promotions> _promotions;

    public Checkout(List<Promotions> promotions)
        => _promotions = promotions;

    public List<Product> Products { get; private set; } = new();

    public void Scan(List<Product> products)
    {
        Products = products;
        Products.ForEach(p => Globals.Logs.Add($"{p} succesfully added"));
    }

    public string Total()
    {
        _promotions.OrderBy(p => p.Priority).ThenBy(p => p.CreateDate).ToList().ForEach(p => p.Apply(Products));
        return $"Total Price: {Globals.Currency}{Products.Select(x => x.Price).Sum().ToString("0.##")}";
    }
}

//If you spend over £75 then you get a 10% discount
class Discount : Promotions
{
    private readonly decimal discountPercentage;
    private readonly int priceToApply;

    /// <summary>
    /// If you spend over £75 then you get a 10% discount
    /// </summary>
    /// <param name="priceToApply">Price that apply current promotion.</param>
    /// <param name="discountPercentage">Discount value in % for promotion applying.</param>
    public Discount(int priceToApply, decimal discountPercentage)
    {
        this.priceToApply = priceToApply;
        this.discountPercentage = (decimal)0.01 * discountPercentage;
    }
    public override void Apply(List<Product> products)
    {
        try
        {
            Globals.Logs.Add($"Try to apply {nameof(Discount)} promotion.");
            var amount = products.Select(x => x.Price).Sum();
            if (amount > priceToApply)
            {
                products.ForEach(x =>
                {
                    x.Price -= x.Price * discountPercentage;
                });
                Globals.Logs.Add($"Promotion {nameof(Discount)} successfully applied.");
            }
        }
        catch (Exception ex)
        {
            Globals.Logs.Add($"Excetion {ex.Message} occures when applytin a {nameof(Discount)} promotion.");
        }
    }
}

//If you buy two or more water bottles then the price drops to £22.99 each
class BottlePriceDrop : Promotions
{
    private readonly decimal waterBottleId;
    private readonly int wBottCount;
    private readonly decimal newWaterPrice;

    /// <summary>
    /// If you buy two or more water bottles then the price drops to £22.99 each
    /// </summary>
    /// <param name="waterBottleId">Water bottle ID.</param>
    /// <param name="wBottCount">Water bottle count to apply this promotion.</param>
    /// <param name="newWaterPrice">New water bottle price after applying promotion.</param>
    public BottlePriceDrop(decimal waterBottleId, int wBottCount, decimal newWaterPrice)
    {
        this.waterBottleId = waterBottleId;
        this.wBottCount = wBottCount;
        this.newWaterPrice = newWaterPrice;
    }
    public override void Apply(List<Product> products)
    {
        try
        {
            Globals.Logs.Add($"Try to apply {nameof(BottlePriceDrop)} promotion.");
            var bottles = products.Where(x => x.Id == waterBottleId);
            if (bottles.Count() >= wBottCount)
            {
                products.ForEach(x =>
                {
                    if (x.Id == waterBottleId)
                    {
                        x.Price = newWaterPrice;
                    }
                });
                Globals.Logs.Add($"Promotion {nameof(BottlePriceDrop)} successfully applied.");
            }
        }
        catch (Exception ex)
        {
            Globals.Logs.Add($"Excetion {ex.Message} occures when applytin a {nameof(BottlePriceDrop)} promotion.");
        }
    }
}