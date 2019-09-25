using AutoMapper;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            Mapper.Initialize(sfg => sfg.AddProfile(new ProductShopProfile()));

            //var userStream = new StreamReader(@"E:\Кодене\ProductShop - Skeleton\ProductShop\Datasets\users.xml");

            //var users = userStream.ReadToEnd();

            //System.Console.WriteLine(ImportUsers(context, users));

            //var productStream = new StreamReader(@"E:\Кодене\ProductShop - Skeleton\ProductShop\Datasets\products.xml");

            //var products = productStream.ReadToEnd();

            //Console.WriteLine(ImportProducts(context, products));

            //var categoryReader = new StreamReader(@"E:\Кодене\ProductShop - Skeleton\ProductShop\Datasets\categories.xml");

            //var categories = categoryReader.ReadToEnd();

            //Console.WriteLine(ImportCategories(context, categories));

            //var catProdReader = new StreamReader(@"E:\Кодене\ProductShop - Skeleton\ProductShop\Datasets\categories-products.xml");

            //var catProds = catProdReader.ReadToEnd();

            //Console.WriteLine(ImportCategoryProducts(context, catProds));

            //Console.WriteLine(GetProductsInRange(context));

            //Console.WriteLine(GetSoldProducts(context));

            //Console.WriteLine(GetCategoriesByProductsCount(context));

            //Console.WriteLine(GetUsersWithProducts(context));
        }

        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            var serializer = new XmlSerializer(typeof(UserImportDto[]), new XmlRootAttribute("Users"));

            var usersDto = (UserImportDto[])serializer.Deserialize(new StringReader(inputXml));

            var users = Mapper.Map<User[]>(usersDto);

            context.Users.AddRange(users);

            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(ProductImportDto[]), new XmlRootAttribute("Products"));

            var productsDto = (ProductImportDto[])ser.Deserialize(new StringReader(inputXml));

            var products = Mapper.Map<Product[]>(productsDto);

            context.Products.AddRange(products);

            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(CategoryImportDto[]), new XmlRootAttribute("Categories"));

            var categoriesDto = (CategoryImportDto[])ser.Deserialize(new StringReader(inputXml));

            var categories = Mapper.Map<Category[]>(categoriesDto);

            context.Categories.AddRange(categories);

            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var ser = new XmlSerializer(typeof(CategoryProductImportDto[]), new XmlRootAttribute("CategoryProducts"));

            var catProductsDto = (CategoryProductImportDto[])ser.Deserialize(new StringReader(inputXml));

            var products = new List<CategoryProduct>();

            foreach (var catprod in catProductsDto)
            {
                if (context.Products.Any(p => p.Id == catprod.ProductId) && context.Categories.Any(c => c.Id == catprod.CategoryId))
                {
                    var catproduct = Mapper.Map<CategoryProduct>(catprod);

                    products.Add(catproduct);
                }
            }

            context.CategoryProducts.AddRange(products);

            context.SaveChanges();

            return $"Successfully imported {products.Count}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .Select(p => new ProductExportDto
                {
                    Name = p.Name,
                    Price = p.Price,
                    Buyer = p.Buyer.FirstName + " " + p.Buyer.LastName
                })
                .OrderBy(p => p.Price)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<ProductExportDto>), new XmlRootAttribute("Products"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            ser.Serialize(new StringWriter(sb), products, namespaces);

            return sb.ToString();
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Count > 0)
                .Select(u => new SellingUserExport
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProductsSold = u.ProductsSold.Select(p => new SoldProductExport
                    {
                        Name = p.Name,
                        Price = p.Price
                    }).ToList()
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<SellingUserExport>), new XmlRootAttribute("Users"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("", "")
            });

            ser.Serialize(new StringWriter(sb), users, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var cats = context.Categories
                .Select(c => new CategoryExport
                {
                    Name = c.Name,
                    Products = c.CategoryProducts.Count,
                    AvgPrice = c.CategoryProducts.Average(p => p.Product.Price),
                    Tot = c.CategoryProducts.Sum(p => p.Product.Price)
                })
                .OrderByDescending(c => c.Products)
                .ThenBy(c => c.Tot)
                .ToList();

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(List<CategoryExport>), new XmlRootAttribute("Categories"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("", "")
            });

            ser.Serialize(new StringWriter(sb), cats, namespaces);

            return sb.ToString().Trim();
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any())
                .Select(u => new ExportSeller
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new ExportSold
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold.Select(p => new SoldProductExport
                        {
                            Name = p.Name,
                            Price = p.Price
                        })
                        .OrderByDescending(p => p.Price)
                        .ToList()
                    }
                })
                .OrderByDescending(u => u.SoldProducts.Count)
                .ToList();

            var correctUsers = new CustomSellersDto
            {
                Count = users.Count,
                Sellers = users
            };

            var sb = new StringBuilder();

            var ser = new XmlSerializer(typeof(CustomSellersDto), new XmlRootAttribute("Users"));

            var namespaces = new XmlSerializerNamespaces(new[]
            {
                new XmlQualifiedName("","")
            });

            ser.Serialize(new StringWriter(sb), correctUsers, namespaces);

            return sb.ToString().Trim();
        }
    }
}