import React from 'react';
import { useOrderHistory } from '../../hooks/useOrderHistory';
import { OrderStatus } from '../../types';
import { Card, CardContent, Badge, Button } from '@/shared/components';
import { Package, ChevronRight, Clock, AlertCircle, Truck, CheckCircle2, X } from 'lucide-react';
import { Link } from 'react-router-dom';
import { cn } from '@/shared/lib/utils';

const statusStyles: Record<OrderStatus, { color: string, icon: any }> = {
  [OrderStatus.Pending]: { color: 'bg-amber-100 text-amber-700 border-amber-200', icon: Clock },
  [OrderStatus.Confirmed]: { color: 'bg-blue-100 text-blue-700 border-blue-200', icon: AlertCircle },
  [OrderStatus.Shipped]: { color: 'bg-purple-100 text-purple-700 border-purple-200', icon: Truck },
  [OrderStatus.Delivered]: { color: 'bg-emerald-100 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
  [OrderStatus.Cancelled]: { color: 'bg-muted text-subtle border-border', icon: X }
};

export function OrderHistory() {
  const { orders, isLoading, isEmpty, error, pagination, selectedStatus, handleStatusFilter, statuses } = useOrderHistory();

  const header = (
    <div className="mb-12 flex items-center gap-4 animate-in fade-in slide-in-from-bottom-4 duration-500">
      <div className="bg-primary/10 p-3 rounded-2xl">
        <Package className="h-8 w-8 text-primary" />
      </div>
      <div>
        <h1 className="text-4xl font-extrabold tracking-tight text-foreground">Order History</h1>
        <p className="text-muted-foreground mt-1">View and track your recent orders.</p>
      </div>
    </div>
  );

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background text-foreground pt-24 pb-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {header}
          <div className="flex flex-col items-center justify-center py-24">
            <Package className="h-12 w-12 text-primary animate-pulse mb-4" />
            <p className="text-muted-foreground">Loading your orders...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-background text-foreground pt-24 pb-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {header}
          <div className="bg-red-50 border border-red-200 p-6 rounded-2xl text-center">
            <p className="text-red-500">{error}</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background text-foreground pt-24 pb-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {header}
        <div className="space-y-6">
          <div className="flex items-center gap-2 overflow-x-auto pb-2 custom-scrollbar">
            <Button
              variant={selectedStatus === undefined ? 'default' : 'outline'}
              size="sm"
              onClick={() => handleStatusFilter(undefined)}
              className={cn("rounded-full whitespace-nowrap", selectedStatus === undefined && "bg-primary text-primary-foreground")}
            >
              All Orders
            </Button>
            {statuses.map(status => (
              <Button
                key={status}
                variant={selectedStatus === status ? 'default' : 'outline'}
                size="sm"
                onClick={() => handleStatusFilter(status)}
                className={cn("rounded-full whitespace-nowrap", selectedStatus === status && "bg-primary text-primary-foreground")}
              >
                {status}
              </Button>
            ))}
          </div>

          {isEmpty ? (
            <Card className="bg-surface border-border border-dashed">
              <CardContent className="flex flex-col items-center justify-center py-24 text-center">
                <Package className="h-12 w-12 text-subtle mb-4" />
                <h3 className="text-xl font-bold mb-2 text-foreground">No orders found</h3>
                <p className="text-muted-foreground mb-6">
                  {selectedStatus 
                    ? `You have no ${selectedStatus.toLowerCase()} orders.` 
                    : "You haven't placed any orders yet."}
                </p>
                <Link to="/products">
                  <Button className="bg-primary text-primary-foreground hover:bg-primary-dark rounded-xl">
                    Start Shopping
                  </Button>
                </Link>
              </CardContent>
            </Card>
          ) : (
            <div className="grid grid-cols-1 gap-4">
              {orders.map((order) => {
                const style = statusStyles[order.status];
                const StatusIcon = style.icon;
                
                return (
                  <Card key={order.id} className="bg-surface border-border hover:shadow-md transition-shadow group">
                    <CardContent className="p-6">
                      <div className="flex flex-col md:flex-row gap-6 md:items-center justify-between">
                        <div className="space-y-3">
                          <div className="flex items-center gap-3">
                            <span className="font-mono text-sm font-bold text-foreground bg-muted px-2 py-1 rounded">
                              #{order.id.split('-')[0]}
                            </span>
                            <div className={cn(
                              "inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider border",
                              style.color
                            )}>
                              <StatusIcon className="h-3 w-3" />
                              {order.status}
                            </div>
                          </div>
                          <div className="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm text-muted-foreground">
                            <span className="flex items-center gap-1.5">
                              <Clock className="h-4 w-4 text-subtle" />
                              {new Date(order.createdAt).toLocaleDateString()}
                            </span>
                            <span className="flex items-center gap-1.5">
                              <Package className="h-4 w-4 text-subtle" />
                              {order.itemCount} {order.itemCount === 1 ? 'item' : 'items'}
                            </span>
                          </div>
                        </div>

                        <div className="flex items-center justify-between md:justify-end gap-6 w-full md:w-auto border-t md:border-none pt-4 md:pt-0">
                          <div className="flex flex-col md:text-right">
                            <span className="text-xs text-subtle font-medium uppercase tracking-wider">Total</span>
                            <span className="text-xl font-black text-foreground">
                              ${order.totalAmount.toFixed(2)}
                            </span>
                          </div>
                          <Link to={`/orders/${order.id}`}>
                            <Button variant="outline" className="rounded-xl group-hover:bg-primary group-hover:text-primary-foreground group-hover:border-primary transition-colors">
                              View Details
                              <ChevronRight className="h-4 w-4 ml-1" />
                            </Button>
                          </Link>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>
          )}

          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <div className="flex justify-center gap-2 mt-8">
              {Array.from({ length: pagination.totalPages }).map((_, i) => {
                const pageNum = i + 1;
                return (
                  <button
                    key={i}
                    onClick={() => pagination.setPage(pageNum)}
                    className={`w-10 h-10 rounded-xl flex items-center justify-center font-medium transition-all ${
                      pagination.currentPage === pageNum
                        ? 'bg-primary text-primary-foreground'
                        : 'bg-surface border border-border hover:bg-muted text-muted-foreground'
                    }`}
                  >
                    {pageNum}
                  </button>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
