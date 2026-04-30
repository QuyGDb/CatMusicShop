import React, { useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

interface AutoScrollRowProps {
  items: any[];
}

export function AutoScrollRow({ items }: AutoScrollRowProps) {
  const navigate = useNavigate();
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const scrollContainer = scrollRef.current;
    if (!scrollContainer) return;

    const interval = setInterval(() => {
      const { scrollLeft, offsetWidth, scrollWidth } = scrollContainer;
      const nextScroll = scrollLeft + 300;

      if (scrollLeft + offsetWidth >= scrollWidth - 10) {
        scrollContainer.scrollTo({ left: 0, behavior: 'smooth' });
      } else {
        scrollContainer.scrollTo({ left: nextScroll, behavior: 'smooth' });
      }
    }, 3000);

    return () => clearInterval(interval);
  }, [items]);

  return (
    <div 
      ref={scrollRef}
      className="flex flex-row overflow-x-auto gap-4 pb-8 snap-x snap-mandatory scrollbar-hide no-scrollbar scroll-smooth"
    >
      {items.map((item) => (
        <div
          key={item.productId}
          onClick={() => navigate(`/products/${item.slug}`)}
          className="flex-shrink-0 w-[240px] md:w-[300px] snap-start group/card relative aspect-square overflow-hidden bg-muted cursor-pointer transition-all duration-500 shadow-xl"
        >
          <img
            src={item.coverUrl}
            alt={item.title}
            className="w-full h-full object-cover transition-transform duration-700 group-hover/card:scale-110"
          />

          <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/20 to-transparent opacity-0 group-hover/card:opacity-100 transition-all duration-500 flex flex-col justify-end p-4">
            <div className="translate-y-2 group-hover/card:translate-y-0 transition-transform duration-500 space-y-0.5">
              <h4 className="font-black text-base leading-tight text-white tracking-tighter drop-shadow-md line-clamp-1">
                {item.title}
              </h4>
              <div className="flex items-center justify-between">
                <p className="text-white/80 text-[10px] font-bold uppercase tracking-wider line-clamp-1">
                  {item.artistName}
                </p>
                <span className="text-white font-black text-xs">
                  ${item.price}
                </span>
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
