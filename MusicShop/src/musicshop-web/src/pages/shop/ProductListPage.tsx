import React from 'react';
import { ProductCard, FilterBar, useProductsList, useProductFilters } from '@/features/products';
import { Loader2, Music2, Filter } from 'lucide-react';


export default function ProductListPage() {
  const {
    products,
    loading,
    error,
    totalItems,
    totalPages,
    currentPage
  } = useProductsList();

  const { setPage } = useProductFilters();

  const [showFilters, setShowFilters] = React.useState(false);

  return (
    <div className="min-h-screen bg-background text-foreground pt-20 sm:pt-24 pb-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">

        {/* Header Area */}
        <div className="mb-8 sm:mb-12 flex flex-col md:flex-row md:items-end justify-between gap-6">
          <div className="space-y-2">
            <h1 className="text-3xl sm:text-4xl font-extrabold tracking-tight bg-gradient-to-r from-foreground to-muted-foreground bg-clip-text text-transparent">
              Explore Collection
            </h1>
            <p className="text-muted-foreground text-sm sm:text-base max-w-xl">
              Discover high-quality vinyl records and CDs from your favorite artists.
            </p>
          </div>
          <div className="flex items-center justify-between md:justify-end gap-4">
            <span className="text-xs sm:text-sm text-subtle font-medium">
              Showing {products.length} of {totalItems} items
            </span>
            <button 
              onClick={() => setShowFilters(!showFilters)}
              className="lg:hidden flex items-center gap-2 px-4 py-2 bg-surface border border-border rounded-xl text-sm font-bold text-foreground"
            >
              <Filter className="h-4 w-4" />
              Filters
            </button>
          </div>
        </div>

        <div className="flex flex-col lg:flex-row gap-8">
          {/* Sidebar Filters */}
          <aside className={`lg:w-64 flex-shrink-0 ${showFilters ? 'block' : 'hidden lg:block'}`}>
            <FilterBar />
          </aside>


          {/* Product Grid */}
          <main className="flex-1">
            {loading ? (
              <div className="flex flex-col items-center justify-center py-24">
                <Loader2 className="h-12 w-12 text-primary animate-spin mb-4" />
                <p className="text-muted-foreground">Loading products...</p>
              </div>
            ) : error ? (
              <div className="bg-red-50 border border-red-200 p-6 rounded-2xl text-center">
                <p className="text-red-500">{error}</p>
              </div>
            ) : (
              <>
                {products.length > 0 ? (
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                    {products.map((product) => (
                      <ProductCard key={product.id} product={product} />
                    ))}
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center py-24 text-center">
                    <Music2 className="h-12 w-12 text-subtle mb-4" />
                    <h3 className="text-xl font-bold mb-2 text-foreground">No products found</h3>
                    <p className="text-muted-foreground">Try adjusting your filters or search query.</p>
                  </div>
                )}

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="mt-12 flex justify-center gap-2">
                    {Array.from({ length: totalPages }).map((_, i) => {
                      const pageNum = i + 1;
                      return (
                        <button
                          key={i}
                          onClick={() => setPage(pageNum)}
                          className={`px-4 py-2 rounded-lg border transition-all ${currentPage === pageNum
                            ? 'bg-primary border-primary text-primary-foreground'
                            : 'border-border bg-surface hover:border-primary text-muted-foreground'
                            }`}
                        >
                          {pageNum}
                        </button>
                      );
                    })}
                  </div>
                )}
              </>
            )}
          </main>
        </div>
      </div>
    </div>
  );
}
