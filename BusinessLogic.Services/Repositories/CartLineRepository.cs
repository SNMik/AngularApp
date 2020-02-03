﻿using AutoMapper;
using BusinessLogic.Interfaces.Repositories;
using DataAccess.Database.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL = BusinessLogic.ModelsDTO;
using DA = DataAccess.Models;

namespace BusinessLogic.Services.Repositories
{
    public class CartLineRepository : IRepository<BL.CartLine>
    {
        private readonly IContextFactory _contextFactory;
        private readonly IMapper _mapper;

        public CartLineRepository(IMapper mapper, IContextFactory contextFactory)
        {
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

        public async Task<ICollection<BL.CartLine>> GetAsync()
        {
            using (var context = _contextFactory.GetProductContext())
            {   
                var entity = await context
                    .CartLines
                    .Include(c => c.Product)
                    .ToListAsync();

                return entity.Select(item =>
                {
                    var mapEntity = _mapper.Map<BL.CartLine>(item);
                    return mapEntity;
                }).ToList();
            }
        }

        public async Task<BL.CartLine> GetByIdAsync(int id)
        {
            using (var context = _contextFactory.GetProductContext())
            {
                var entity = await context.CartLines.FirstOrDefaultAsync(x => x.Id == id);
                return _mapper.Map<BL.CartLine>(entity);
            }
        }

        public async Task<int> SaveAsync(BL.CartLine entity)
        {
            try
            {
                if (entity == null) return 0;

                using (var context = _contextFactory.GetProductContext())
                {
                    var entityModel = await context
                    .CartLines
                    .FirstOrDefaultAsync(item => item.Id.Equals(entity.Id));

                    if (entityModel == null)
                    {
                        entityModel = new DA.CartLine();
                        MapForUpdateEntity(entity, entityModel,context);
                        
                        await context.CartLines.AddAsync(entityModel);
                    }
                    else
                    {
                        MapForUpdateEntity(entity, entityModel,context);
                    }

                    context.SaveChanges();
                    return entityModel.Id;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                using (var context = _contextFactory.GetProductContext())
                {
                    var entityModel = await context
                        .CartLines
                        .FirstOrDefaultAsync(item => item.Id.Equals(id));

                    if (entityModel == null) return;

                    await Task.Run(() => context.CartLines.Remove(entityModel));

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ICollection<BL.CartLine>> FindByAsync(Func<BL.CartLine, bool> predicate)
        {
            try
            {
                using (var context = _contextFactory.GetProductContext())
                {
                    var entity = await context
                        .CartLines                         
                        .Include(c => c.Product)
                        .ToListAsync();

                    return entity.Select(item =>
                                        {
                                            var mapEntity = _mapper.Map<BL.CartLine>(item);
                                            return mapEntity;
                                        })
                                .Where(predicate).ToList();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    
        private void MapForUpdateEntity(BL.CartLine entity, DA.CartLine daEntity, IProductContext context)
        {
            daEntity.Id = entity.Id;
            daEntity.Price = entity.Price;
            daEntity.Quantity = entity.Quantity;
            daEntity.User = context.Users.Find(entity.User.Id);
            daEntity.Product = context.Products.Find(entity.Product.Id);
        }       
    }
}
