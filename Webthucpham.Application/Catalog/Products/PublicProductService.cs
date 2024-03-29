﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Webthucpham.Application.Catalog.Products.Dtos;
using Webthucpham.Application.Catalog.Products.Dtos.Manage;
using Webthucpham.Application.Dtos;
using Webthucpham.Data.EF;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Webthucpham.Application.Catalog.Products
{
    public class PublicProductService : IPublicProductService
    {
        private readonly WebthucphamDbContext _context; //đọc
        public PublicProductService(WebthucphamDbContext context)
        {
            _context = context; //gán 1 lần
        }
        public async Task<PagedResult<ProductViewModel>> GetAllByCategory(Dtos.Public.GetProductPagingRequest request)
        {
            //using linq

            //1 : select join
            var query = from p in _context.Products
                        join pt in _context.ProductTranslations on p.Id equals pt.ProductId // với bảng Translation
                        join pic in _context.ProductInCategories on p.Id equals pic.ProductId //với bảng ProcutInCategory
                        join c in _context.Categories on pic.CategoryId equals c.Id           // với bảng Category
                        select new { p, pt, pic };


            // 2 : filter

            if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
            {
                query = query.Where(p => p.pic.CategoryId == request.CategoryId);
            }
            //3 : Paging
            int totalRow = await query.CountAsync();
            //add data
            var data = await query.Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductViewModel()
                {
                    Id = x.p.Id,
                    Name = x.pt.Name,
                    DateCreated = x.p.DateCreated,
                    Description = x.pt.Description,
                    Details = x.pt.Details,
                    LanguageId = x.pt.LanguageId,
                    OriginalPrice = x.p.OriginalPrice,
                    Price = x.p.Price,
                    SeoAlias = x.pt.SeoAlias,
                    SeoDescription = x.pt.SeoDescription,
                    Stock = x.p.Stock,
                    ViewCount = x.p.ViewCount

                }).ToListAsync();

            // 4 :  Select and Project
            var pagedResult = new PagedResult<ProductViewModel>
            {
                TotalRecord = totalRow,
                Items = data
            };
            return pagedResult;

        }
    }
}
