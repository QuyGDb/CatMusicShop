import { Filter, Eye, MoreVertical, Calendar, XCircle } from 'lucide-react';
import { Button, Card, CardContent, ManagementLayout, EmptyState } from '@/shared/components';
import { cn } from '@/shared/lib/utils';
import { OrderDetailsModal } from './OrderDetailsModal';
import { useOrderManagement } from '../../hooks/useOrderManagement';
import { OrderStatus } from '../../types';

/**
 * Presentational component for order and fulfillment management.
 * Logic is delegated to useOrderManagement hook.
 */
export function OrderManagement() {
  const {
    orders,
    isLoading,
    error,
    stats,
    statusStyles,
    selectedOrderId,
    searchTerm,
    actions
  } = useOrderManagement();

  const isEmpty = orders.length === 0;

  return (
    <ManagementLayout
      title="Orders & Fulfillment"
      subtitle="Monitor customer purchases and manage the lifecycle of every shipment."
      isLoading={isLoading}
      error={error}
      isEmpty={isEmpty}
      searchPlaceholder="Search by Order ID, name or email..."
      searchQuery={searchTerm}
      onSearchChange={actions.setSearchTerm}
      emptyState={
        <EmptyState 
          icon={Calendar} 
          title="No orders yet" 
          description="Your shop is waiting for its first customer. Let's make some sales!" 
        />
      }
      filterContent={null}
    >
      {/* Summary Stats */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        {stats.map((stat, i) => (
          <Card key={i} className="bg-surface border-border">
            <CardContent className="p-6 flex items-center gap-4">
              <div className={cn("p-3 rounded-2xl", stat.bg)}>
                <stat.icon className={cn("h-6 w-6", stat.color)} />
              </div>
              <div>
                <p className="text-xs font-black uppercase tracking-widest text-subtle">{stat.label}</p>
                <p className="text-2xl font-black text-foreground">{stat.value}</p>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Orders Table */}
      <Card className="bg-surface border-border overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="bg-muted/30 border-b border-border">
                <th className="p-5 text-xs font-black uppercase tracking-widest text-subtle">Order ID</th>
                <th className="p-5 text-xs font-black uppercase tracking-widest text-subtle">Customer</th>
                <th className="p-5 text-xs font-black uppercase tracking-widest text-subtle">Date</th>
                <th className="p-5 text-xs font-black uppercase tracking-widest text-subtle">Total</th>
                <th className="p-5 text-xs font-black uppercase tracking-widest text-subtle">Status</th>
                <th className="p-5 text-xs font-black uppercase tracking-widest text-subtle text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {orders.map((order) => {
                const style = statusStyles[order.status];
                const StatusIcon = style.icon;
                return (
                  <tr key={order.id} className="border-b border-border/50 hover:bg-muted/10 transition-colors group">
                    <td className="p-5">
                      <span className="font-mono text-sm font-bold text-foreground">#{order.id}</span>
                    </td>
                    <td className="p-5">
                      <div className="flex flex-col">
                        <span className="font-bold text-foreground">{order.recipientName}</span>
                        <span className="text-xs text-muted-foreground">{order.email}</span>
                      </div>
                    </td>
                    <td className="p-5 text-sm text-muted-foreground">
                      {new Date(order.createdAt).toLocaleDateString()}
                    </td>
                    <td className="p-5 font-bold text-foreground">
                      ${order.totalAmount.toFixed(2)}
                    </td>
                    <td className="p-5">
                      <div className={cn(
                        "inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-wider border",
                        style.color
                      )}>
                        <StatusIcon className="h-3 w-3" />
                        {order.status}
                      </div>
                    </td>
                    <td className="p-5 text-right">
                      <div className="flex justify-end gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-9 w-9 text-muted-foreground hover:text-primary rounded-xl"
                          onClick={() => actions.openDetails(order)}
                        >
                          <Eye className="h-4 w-4" />
                        </Button>
                        {order.status !== OrderStatus.Cancelled && (
                          <Button 
                            variant="ghost" 
                            size="icon" 
                            className="h-9 w-9 text-muted-foreground hover:text-destructive rounded-xl"
                            onClick={() => actions.handleCancel(order.id)}
                          >
                            <XCircle className="h-4 w-4" />
                          </Button>
                        )}
                        <Button variant="ghost" size="icon" className="h-9 w-9 text-muted-foreground rounded-xl">
                          <MoreVertical className="h-4 w-4" />
                        </Button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Detail Modal */}
      {selectedOrderId && (
        <OrderDetailsModal
          orderId={selectedOrderId}
          onClose={actions.closeDetails}
        />
      )}
    </ManagementLayout>
  );
}
