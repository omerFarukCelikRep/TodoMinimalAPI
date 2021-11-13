using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conntectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ApiDbContext>(options =>
{
    options.UseSqlite(conntectionString);
});

//builder.Services.AddSingleton<ItemRepository>();

var app = builder.Build();

app.MapGet("/items", async (ApiDbContext context) =>
{
    return await context.Items.ToListAsync();
});

app.MapPost("/items", async (ApiDbContext context, Item item) =>
{
    if (await context.Items.FirstOrDefaultAsync(a => a.Id == item.Id) != null)
    {
        return Results.BadRequest();
    }

    context.Items.Add(item);
    await context.SaveChangesAsync();

    return Results.Created($"/items/{item.Id}",item);
});

app.MapGet("/items/{id}", async (ApiDbContext context, int id) =>
{
    var item = await context.Items.FindAsync(id);

    return item == null ? Results.NotFound() : Results.Ok(item);
});

app.MapPut("/items/{id}", async (ApiDbContext context, int id, Item item) =>
{
    var existItem = await context.Items.FirstOrDefaultAsync(a => a.Id == item.Id);
    if (existItem == null)
    {
        return Results.NotFound();
    }

    existItem.Title = item.Title;
    existItem.IsCompleted = item.IsCompleted;

    await context.SaveChangesAsync();

    return Results.Ok(item);
});

app.MapDelete("/items/{id}", async (ApiDbContext context, int id) =>
{
    var existItem = await context.Items.FirstOrDefaultAsync(a => a.Id == id);
    if (existItem == null)
    {
        return Results.NotFound();
    }

    context.Items.Remove(existItem);
    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/", () => "Hello from Minimal API");

app.Run();

class Item
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

//record Item(int id, string title, bool IsCompleted);

//class ItemRepository
//{
//    private Dictionary<int, Item> items = new();

//    public ItemRepository()
//    {
//        Item item1 = new(1, "Go to the gym", false);
//        Item item2 = new(2, "Drink Water", true);
//        Item item3 = new(3, "Watch Movie", false);

//        items.Add(item1.id, item1);
//        items.Add(item2.id, item2);
//        items.Add(item3.id, item3);
//    }

//    public IEnumerable<Item> GetAll() => items.Values;

//    public Item GetById(int id)
//    {
//        if (items.ContainsKey(id))
//        {
//            return items[id];
//        }

//        return null;
//    }

//    public void Add(Item item)
//    {
//        items.Add(item.id, item);
//    }

//    public void Update(Item item)
//    {
//        items[item.id] = item;
//    }

//    public void Delete(int id) => items.Remove(id);
//}

class ApiDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {

    }
}