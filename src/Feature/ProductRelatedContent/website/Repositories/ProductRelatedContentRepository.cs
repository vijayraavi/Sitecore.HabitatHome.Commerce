﻿using Sitecore.Commerce.Entities.Products;
using Sitecore.Commerce.XA.Feature.Catalog.Models;
using Sitecore.Commerce.XA.Feature.Catalog.Repositories;
using Sitecore.Commerce.XA.Foundation.Catalog.Managers;
using Sitecore.Commerce.XA.Foundation.Common;
using Sitecore.Commerce.XA.Foundation.Common.Models;
using Sitecore.Commerce.XA.Foundation.Common.Providers;
using Sitecore.Commerce.XA.Foundation.Common.Search;
using Sitecore.Commerce.XA.Foundation.Connect;
using Sitecore.Commerce.XA.Foundation.Connect.Managers;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.HabitatHome.Feature.ProductRelatedContent.Models;
using Sitecore.HabitatHome.Foundation.Catalog.Managers;
using Sitecore.Links;
using Sitecore.Resources.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.HabitatHome.Feature.ProductRelatedContent.Repositories
{
    public class ProductRelatedContentRepository : BaseCatalogRepository, IProductRelatedContentRepository
    {
        public ProductRelatedContentRepository(IModelProvider modelProvider, IStorefrontContext storefrontContext, ISiteContext siteContext, ISearchInformation searchInformation, ISearchManager searchManager, ICatalogManager catalogManager, ICatalogUrlManager catalogUrlManager, IVariantDefinitionProvider variantDefinitionProvider, IProductRelatedContentManager productRelatedContentManager) : base(modelProvider, storefrontContext, siteContext, searchInformation, searchManager, catalogManager, catalogUrlManager)
        {
            _productRelatedContentManager = productRelatedContentManager;
        }
        private IProductRelatedContentManager _productRelatedContentManager;

        public virtual CatalogItemRenderingModel GetProductRelatedContentRenderingModel(IVisitorContext visitorContext)
        {
            return this.GetProduct(visitorContext);
        }

        public virtual IEnumerable<RelatedProductJsonResult> GetRelatedProducts(IModelProvider modelProvider, IStorefrontContext storefrontContext, IVisitorContext visitorContext, string productId)
        {            
            List<RelatedProductJsonResult> associatedProductItems = new List<RelatedProductJsonResult>();
            List<Item> associatedProducts = _productRelatedContentManager.GetAssociatedProducts(productId, "RelatedProduct").ToList();
            if (associatedProducts.Count() > 0)
            {
                associatedProducts.Take(3).ToList().ForEach((Action<Item>)(item =>
                {
                    associatedProductItems.Add(BuildRelatedProductResult(item, visitorContext));

                }));
            }
            return associatedProductItems;
        }

        public RelatedProductJsonResult BuildRelatedProductResult(Item item, IVisitorContext visitorContext)
        {
            
            CommerceStorefront currentStorefront = this.StorefrontContext.CurrentStorefront;
            string imageUrl = String.Empty;
            MultilistField imagesFld = (MultilistField)item.Fields["Images"];
            if (imagesFld != null && !String.IsNullOrEmpty(imagesFld.Value))
            {
                MediaItem imageItem = Sitecore.Context.Database.GetItem(imagesFld.TargetIDs[0]);
                imageUrl = imageItem != null ? MediaManager.GetMediaUrl(imageItem) : " ";
            }
            Sitecore.Commerce.XA.Foundation.Connect.Entities.ProductEntity productModel = this.ModelProvider.GetModel<Sitecore.Commerce.XA.Foundation.Connect.Entities.ProductEntity>();
            productModel.Initialize(currentStorefront, item, null);

            this.CatalogManager.GetProductPrice(currentStorefront, visitorContext, productModel);

            return new RelatedProductJsonResult()
            {
                ProductName = item.DisplayName,
                ImageSrc = imageUrl,
                ListPrice = productModel.ListPrice.HasValue ? productModel.ListPrice.Value.ToString() : "0.00",
                ProductUrl = item.ID.ToString().Equals(currentStorefront.GiftCardProductId, StringComparison.OrdinalIgnoreCase) ? currentStorefront.GiftCardPageLink : LinkManager.GetDynamicUrl(item)
            };
        }

        public virtual IEnumerable<RelatedProductJsonResult> GetCrossSellProducts(IModelProvider modelProvider, IStorefrontContext storefrontContext, IVisitorContext visitorContext, string productId)
        {
            List<RelatedProductJsonResult> associatedProductItems = new List<RelatedProductJsonResult>();
            List<Item> associatedProducts = _productRelatedContentManager.GetAssociatedProducts(productId, "CrossSellProduct").ToList();
            if (associatedProducts.Count() > 0)
            {
                associatedProducts.Take(3).ToList().ForEach((Action<Item>)(item =>
                {
                    associatedProductItems.Add(BuildRelatedProductResult(item, visitorContext));

                }));
            }
            return associatedProductItems;
        }

        public virtual IEnumerable<RelatedProductJsonResult> GetUpSellProducts(IModelProvider modelProvider, IStorefrontContext storefrontContext, IVisitorContext visitorContext, string productId)
        {
            List<RelatedProductJsonResult> associatedProductItems = new List<RelatedProductJsonResult>();
            List<Item> associatedProducts = _productRelatedContentManager.GetAssociatedProducts(productId, "UpSellProduct").ToList();
            if (associatedProducts.Count() > 0)
            {
                associatedProducts.Take(3).ToList().ForEach((Action<Item>)(item =>
                {
                    associatedProductItems.Add(BuildRelatedProductResult(item, visitorContext));

                }));
            }
            return associatedProductItems;
        }


        public virtual IEnumerable<ProductDocumentJsonResult> GetProductDocuments(IModelProvider modelProvider, IStorefrontContext storefrontContext, IVisitorContext visitorContext, string productId)
        {
            List<ProductDocumentJsonResult> productDocuments = new List<ProductDocumentJsonResult>();
            List<Item> associatedDocuments = _productRelatedContentManager.GetProductContent(productId).ToList();

            associatedDocuments.ForEach((Action<Item>)(productMediaItem =>
            {
                productDocuments.Add(new ProductDocumentJsonResult()
                {
                    DocumentName = productMediaItem["Title"],
                    Description = productMediaItem["Description"],
                    DocumentType = productMediaItem["ContentType"],
                    ImageUrl = ((ImageField)productMediaItem.Fields["Thumbnail"]).MediaItem != null ? Resources.Media.MediaManager.GetMediaUrl(((ImageField)productMediaItem.Fields["Thumbnail"]).MediaItem) : "",
                    DocumentUrl = ((LinkField)productMediaItem.Fields["Link"]).IsMediaLink ? Resources.Media.MediaManager.GetMediaUrl(((LinkField)productMediaItem.Fields["Link"]).TargetItem) : ((LinkField)productMediaItem.Fields["Link"]).GetFriendlyUrl()
                });
            }));
            return productDocuments;
        }
        
    }
}