import { ArrowLeft, Package, Plus, Settings, Tag, ShieldCheck, AlertCircle, Loader2, Trash2, Edit3, Layers, ExternalLink } from 'lucide-react';
import { Button, Card, CardHeader, CardTitle, CardContent, Skeleton } from '@/shared/components';
import { cn } from '@/shared/lib/utils';
import { ReleaseFormat } from '../../types';
import { useProductAdminDetails } from '../../hooks/useProductAdminDetails';

interface ProductAdminDetailsProps {
   productId: string;
}

/**
 * Presentational component for product administration details.
 * Logic is delegated to useProductAdminDetails hook.
 */
export function ProductAdminDetails({ productId }: ProductAdminDetailsProps) {
   const {
      product,
      isLoading,
      error,
      actions
   } = useProductAdminDetails({ productId });

   if (isLoading) {
      return (
         <div className="space-y-8 animate-pulse">
            <Skeleton className="h-10 w-48 bg-muted rounded-xl" />
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
               <Skeleton className="lg:col-span-2 h-[400px] bg-muted rounded-3xl" />
               <Skeleton className="h-[400px] bg-muted rounded-3xl" />
            </div>
         </div>
      );
   }

   if (error || !product) {
      return (
         <div className="flex flex-col items-center justify-center py-20 space-y-4">
            <AlertCircle className="h-12 w-12 text-red-500" />
            <p className="text-xl font-bold">Product not found</p>
            <Button onClick={actions.back}>Back to Inventory</Button>
         </div>
      );
   }

   return (
      <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
         {/* Header */}
         <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div className="flex flex-col gap-2">
               <button
                  onClick={actions.back}
                  className="flex items-center gap-2 text-xs font-bold text-muted-foreground hover:text-primary transition-colors group w-fit"
               >
                  <ArrowLeft className="h-3 w-3 group-hover:-translate-x-1 transition-transform" />
                  BACK TO INVENTORY
               </button>
               <div className="flex items-center gap-4">
                  <h1 className="text-4xl font-extrabold tracking-tight text-foreground">{product.name}</h1>
                  <span className="px-3 py-1 bg-primary/10 border border-primary/20 rounded-full text-xs font-bold text-primary uppercase">
                     {ReleaseFormat[product.format]}
                  </span>
               </div>
            </div>

            <div className="flex gap-3">
               <Button className="h-12 px-6 rounded-xl bg-surface border border-border text-foreground hover:bg-muted flex gap-2" onClick={actions.viewStore}>
                  <ExternalLink className="h-4 w-4" />
                  View Store
               </Button>
            </div>
         </div>

         <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Left Column: Product Info & Attributes */}
            <div className="lg:col-span-2 space-y-8">
               <Card className="bg-surface border-border shadow-sm rounded-3xl overflow-hidden">
                  <CardHeader className="p-8 border-b border-border flex flex-row items-center justify-between bg-muted/5">
                     <div className="flex items-center gap-3">
                        <div className="bg-amber-500/10 p-2 rounded-xl">
                           <Tag className="h-5 w-5 text-amber-600" />
                        </div>
                        <div>
                           <h3 className="text-lg font-bold">Product Pricing & Stock</h3>
                           <p className="text-xs text-muted-foreground">Manage the active SKU details.</p>
                        </div>
                     </div>
                  </CardHeader>
                  <CardContent className="p-8 space-y-6">
                     <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                        <div className="space-y-2">
                           <label className="text-[10px] font-black uppercase tracking-widest text-subtle">Base Price</label>
                           <div className="text-3xl font-black text-foreground">${product.price.toFixed(2)}</div>
                        </div>
                        <div className="space-y-2">
                           <label className="text-[10px] font-black uppercase tracking-widest text-subtle">Inventory Count</label>
                           <div className="flex items-center gap-3">
                              <div className="text-3xl font-black text-foreground">{product.stockQty}</div>
                              <span className={cn(
                                 "px-2 py-0.5 rounded text-[10px] font-black uppercase",
                                 product.stockQty > 10 ? "bg-emerald-500/10 text-emerald-600" : product.stockQty > 0 ? "bg-amber-500/10 text-amber-600" : "bg-red-500/10 text-red-600"
                              )}>
                                 {product.stockQty > 0 ? 'In Stock' : 'Out of Stock'}
                              </span>
                           </div>
                        </div>
                     </div>

                     <div className="pt-6 border-t border-border">
                        <div className="flex items-center gap-4">
                           <div className="flex items-center gap-2 px-3 py-1.5 bg-muted/50 rounded-xl border border-border">
                              <ShieldCheck className={cn("h-4 w-4", product.isSigned ? "text-emerald-500" : "text-muted-foreground/30")} />
                              <span className="text-xs font-bold">{product.isSigned ? 'Autographed Edition' : 'Standard Edition'}</span>
                           </div>
                           <div className="flex items-center gap-2 px-3 py-1.5 bg-muted/50 rounded-xl border border-border">
                              <AlertCircle className={cn("h-4 w-4", product.isAvailable ? "text-emerald-500" : "text-red-500")} />
                              <span className="text-xs font-bold">{product.isAvailable ? 'Available for Purchase' : 'Currently Unavailable'}</span>
                           </div>
                        </div>
                     </div>
                  </CardContent>
               </Card>

               {/* Attributes Card */}
               {(product.vinylAttributes || product.cdAttributes || product.cassetteAttributes) && (
                  <Card className="bg-surface border-border shadow-sm rounded-3xl overflow-hidden">
                     <CardHeader className="p-8 border-b border-border bg-muted/5">
                        <div className="flex items-center gap-3">
                           <div className="bg-primary/10 p-2 rounded-xl">
                              <Layers className="h-5 w-5 text-primary" />
                           </div>
                           <div>
                              <h3 className="text-lg font-bold">Physical Specifications</h3>
                              <p className="text-xs text-muted-foreground">Format-specific attributes and details.</p>
                           </div>
                        </div>
                     </CardHeader>
                     <CardContent className="p-8">
                        <div className="grid grid-cols-2 sm:grid-cols-3 gap-6">
                           {product.vinylAttributes && (
                              <>
                                 <AttributeItem label="Disc Color" value={product.vinylAttributes.discColor} />
                                 <AttributeItem label="Weight" value={product.vinylAttributes.weightGrams ? `${product.vinylAttributes.weightGrams}g` : null} />
                                 <AttributeItem label="Speed" value={product.vinylAttributes.speedRpm ? `${product.vinylAttributes.speedRpm} RPM` : null} />
                                 <AttributeItem label="Disc Count" value={product.vinylAttributes.discCount} />
                                 <AttributeItem label="Sleeve" value={product.vinylAttributes.sleeveType} />
                              </>
                           )}
                           {product.cdAttributes && (
                              <>
                                 <AttributeItem label="Edition" value={product.cdAttributes.edition} />
                                 <AttributeItem label="Japan Edition" value={product.cdAttributes.isJapanEdition ? 'Yes' : 'No'} />
                              </>
                           )}
                           {product.cassetteAttributes && (
                              <>
                                 <AttributeItem label="Tape Color" value={product.cassetteAttributes.tapeColor} />
                                 <AttributeItem label="Edition" value={product.cassetteAttributes.edition} />
                              </>
                           )}
                        </div>
                     </CardContent>
                  </Card>
               )}
            </div>

            {/* Right Column: Master Info Summary */}
            <div className="space-y-8">
               <Card className="bg-surface border-border shadow-sm rounded-3xl overflow-hidden">
                  <CardHeader className="p-6 border-b border-border bg-muted/10">
                     <div className="flex items-center gap-3">
                        <Settings className="h-5 w-5 text-primary" />
                        <h3 className="text-lg font-bold italic tracking-tight">Listing Specs</h3>
                     </div>
                  </CardHeader>
                  <CardContent className="p-6 space-y-6">
                     <div className="aspect-square rounded-2xl bg-muted border border-border overflow-hidden">
                        {product.coverUrl ? (
                           <img src={product.coverUrl} className="w-full h-full object-cover" />
                        ) : (
                           <div className="w-full h-full flex items-center justify-center">
                              <Package className="h-10 w-10 text-muted-foreground/20" />
                           </div>
                        )}
                     </div>

                     <div className="space-y-4">
                        <div className="flex justify-between items-center text-sm">
                           <span className="text-muted-foreground font-medium">Status</span>
                           <span className={cn(
                              "px-2 py-1 rounded-lg text-[10px] font-black uppercase tracking-widest",
                              product.isActive ? "bg-emerald-500/10 text-emerald-600" : "bg-red-500/10 text-red-600"
                           )}>
                              {product.isActive ? 'Active' : 'Archived'}
                           </span>
                        </div>
                        <div className="flex justify-between items-center text-sm">
                           <span className="text-muted-foreground font-medium">Format</span>
                           <span className="text-foreground font-black uppercase text-[11px]">{ReleaseFormat[product.format]}</span>
                        </div>
                        {product.isLimited && (
                           <div className="flex justify-between items-center text-sm">
                              <span className="text-amber-600 font-bold flex items-center gap-1.5 underline decoration-amber-200 decoration-2">
                                 <ShieldCheck className="h-3.5 w-3.5" />
                                 Limited Ed.
                              </span>
                              <span className="text-foreground font-black">{product.limitedQty} units</span>
                           </div>
                        )}
                     </div>


                  </CardContent>
               </Card>
            </div>
         </div>
      </div>
   );
}

function AttributeItem({ label, value }: { label: string; value?: string | number | null }) {
   if (!value) return null;
   return (
      <div className="space-y-1">
         <p className="text-[10px] font-black uppercase tracking-widest text-subtle">{label}</p>
         <p className="text-sm font-bold text-foreground capitalize">{value.toString()}</p>
      </div>
   );
}
