import { FeaturedCollections } from '@/features/curation/components/storefront/FeaturedCollections';
import { MusicHeroScene } from '@/features/threejs';

export default function HomePage() {
  return (
    <div className="flex flex-col min-h-screen">
      <div className="flex flex-col lg:flex-row w-full h-[60vh] lg:h-screen overflow-hidden bg-[#09090b]">
        <div className="flex-1 h-[60vh] lg:h-full relative">
          <MusicHeroScene />
        </div>
        <div className="hidden lg:block w-[380px] h-full border-l border-white/5 bg-black">
          <iframe 
            title="Spotify Playlist"
            style={{ borderRadius: '12px' }} 
            src="https://open.spotify.com/embed/playlist/1HYRmT9RhRJ8YCEcPyNesn?utm_source=generator&theme=0" 
            width="100%" 
            height="100%" 
            frameBorder="0" 
            allowFullScreen={true} 
            allow="autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture" 
            loading="lazy"
          ></iframe>
        </div>
      </div>
      <div className="container mx-auto px-4 pb-32">
        <FeaturedCollections />
      </div>
    </div>
  );
}
