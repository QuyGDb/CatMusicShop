import React from 'react';
import { useFeaturedCollections } from '../../hooks/useFeaturedCollections';
import { AutoScrollRow } from './AutoScrollRow';

export function FeaturedCollections() {
  const { collections, isLoading } = useFeaturedCollections();

  if (isLoading) {
    return (
      <div className="space-y-12 py-12 animate-pulse">
        {[1, 2].map((i) => (
          <div key={i} className="space-y-10">
            <div className="h-12 w-64 bg-muted rounded-lg" />
            <div className="flex flex-row gap-4 overflow-hidden">
              {[1, 2, 3, 4].map((j) => (
                <div key={j} className="flex-shrink-0 w-[280px] aspect-square rounded-lg bg-muted" />
              ))}
            </div>
          </div>
        ))}
      </div>
    );
  }

  if (collections.length === 0) return null;

  return (
    <section className="space-y-12 py-12">
      {collections.map((collection, index) => (
        <div key={collection.id} className="group relative">
          {/* Section Header */}
          <div className="flex flex-col md:flex-row md:items-end justify-between mb-10 gap-6">
            <div className="space-y-4 max-w-2xl">
              <h2 className="text-2xl md:text-3xl font-black tracking-tighter text-foreground leading-tight uppercase italic">
                {collection.title}
              </h2>
              {collection.description && (
                <p className="text-lg text-muted-foreground leading-relaxed">
                  {collection.description}
                </p>
              )}
            </div>
          </div>

          {/* Auto Scrollable Row */}
          <AutoScrollRow items={collection.items} />

          {/* Decorative background element */}
          <div className={`absolute -z-10 blur-[120px] opacity-20 w-[500px] h-[500px] rounded-full transition-all duration-1000 group-hover:opacity-30 ${
            index % 2 === 0 ? "bg-orange-500/40 -left-48 -top-24" : "bg-purple-500/40 -right-48 -bottom-24"
          }`} />
        </div>
      ))}
    </section>
  );
}
