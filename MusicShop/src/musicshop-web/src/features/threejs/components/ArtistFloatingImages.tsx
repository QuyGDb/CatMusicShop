import React from 'react';
import { Image } from '@react-three/drei';

import beatlesImg from '@/assets/textures/posters/The Beatles-1462762596.jpg';
import abstractImg from '@/assets/textures/posters/ab67616d00001e02d9194aa18fa4c9362b47464f.jpg';
import scenicImg from '@/assets/textures/posters/Scenictour.webp';
import techImg from '@/assets/textures/posters/608963336.jpg.webp';
import defaultImg from '@/assets/textures/posters/image.jpg';
import eImg from '@/assets/textures/posters/e.jpg';

export function ArtistFloatingImages() {
  const settings = [
    {
      id: 1,
      url: beatlesImg,
      pos: [33.2, 14.0, 0.0],
      scale: [10.0, 10.0],
      rot: [0.00, 4.71, 0.00]
    },
    {
      id: 2,
      url: abstractImg,
      pos: [33.2, 14.0, -15.4],
      scale: [10.0, 10.0],
      rot: [0.00, 4.71, 0.00]
    },
    {
      id: 3,
      url: scenicImg,
      pos: [-32.6, 14.0, 0.0],
      scale: [10.0, 10.0],
      rot: [0.00, -4.71, 0.00]
    },
    {
      id: 4,
      url: techImg,
      pos: [3.4, 14.0, 15.2],
      scale: [10.0, 10.0],
      rot: [0.00, 3.14, 0.00]
    },
    {
      id: 5,
      url: defaultImg,
      pos: [-32.6, 14.0, -12.4],
      scale: [10.0, 10.0],
      rot: [0.00, -4.71, 0.00]
    },
    {
      id: 6,
      url: eImg,
      pos: [-11.2, 14.0, 15.2],
      scale: [10.0, 10.0],
      rot: [0.00, 3.14, 0.00]
    },
  ];

  return (
    <group>
      {settings.map((artist) => (
        <Image
          key={artist.id}
          url={artist.url}
          position={artist.pos as [number, number, number]}
          scale={artist.scale as [number, number]}
          rotation={artist.rot as [number, number, number]}
          transparent
          opacity={0.7}
        />
      ))}
    </group>
  );
}
