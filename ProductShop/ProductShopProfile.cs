using AutoMapper;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            this.CreateMap<UserImportDto, User>();

            this.CreateMap<ProductImportDto, Product>();

            this.CreateMap<CategoryImportDto, Category>();

            this.CreateMap<CategoryProductImportDto, CategoryProduct>();

            this.CreateMap<Product, ProductExportDto>();

            this.CreateMap<Product, SoldProductExport>();

            this.CreateMap<User, SellingUserExport>();
        }
    }
}
