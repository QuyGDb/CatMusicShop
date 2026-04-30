import { Globe, UserCircle, Edit2, Trash2, Loader2 } from 'lucide-react';
import { Button, Card, CardContent } from '@/shared/components';
import { Artist } from '@/features/catalog/types';

interface ArtistCardProps {
  artist: Artist;
  onEdit: (artist: Artist) => void;
  onDelete: (id: string) => void;
  isDeleting: boolean;
}

export function ArtistCard({ artist, onEdit, onDelete, isDeleting }: ArtistCardProps) {
  return (
    <Card className="bg-surface border-border overflow-hidden hover:shadow-xl transition-all group relative">
      <div className="aspect-square w-full relative overflow-hidden bg-muted/30">
        {artist.imageUrl ? (
          <img
            src={artist.imageUrl}
            alt={artist.name}
            className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-700"
          />
        ) : (
          <div className="w-full h-full bg-muted flex items-center justify-center">
            <UserCircle className="h-20 w-20 text-muted-foreground/20" />
          </div>
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-black/90 via-black/20 to-transparent" />
        <div className="absolute bottom-4 left-4 right-4">
          <div className="flex flex-wrap gap-1 mb-2">
            {artist.genres.slice(0, 2).map(genre => (
              <span key={genre.id} className="px-2 py-0.5 bg-primary/20 backdrop-blur-md border border-primary/20 rounded text-[9px] font-black uppercase tracking-tighter text-primary">
                {genre.name}
              </span>
            ))}
            {artist.genres.length > 2 && (
              <span className="px-2 py-0.5 bg-muted/20 backdrop-blur-md border border-border/20 rounded text-[9px] font-black uppercase tracking-tighter text-white">
                +{artist.genres.length - 2} More
              </span>
            )}
          </div>
          <h3 className="text-xl font-bold text-white tracking-tight leading-tight">{artist.name}</h3>
          <div className="flex items-center gap-1.5 text-white/60 text-xs mt-1">
            <Globe className="h-3 w-3" />
            {artist.country}
          </div>
        </div>
      </div>
      <CardContent className="p-4 flex items-center justify-between bg-surface">
        <p className="text-xs text-muted-foreground line-clamp-1 flex-1 pr-4">
          {artist.bio || 'No biography available.'}
        </p>
        <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity shrink-0">
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-muted-foreground hover:text-primary"
            onClick={() => onEdit(artist)}
          >
            <Edit2 className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-muted-foreground hover:text-red-500"
            onClick={() => onDelete(artist.id)}
            disabled={isDeleting}
          >
            {isDeleting ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Trash2 className="h-4 w-4" />
            )}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
