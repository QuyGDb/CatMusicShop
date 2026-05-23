import React, { useRef, type ElementRef } from 'react';
import * as THREE from 'three';
import { Canvas } from '@react-three/fiber';
import { Sky, OrbitControls, Environment, SpotLight, PerspectiveCamera } from '@react-three/drei';
import { EffectComposer, Bloom } from '@react-three/postprocessing';
import { useMusicHeroScene } from '../hooks/useMusicHeroScene';
import { Particles } from './Particles';
import { HeroText } from './HeroText';
import { Model } from './Model';
import { ArtistFloatingImages } from './ArtistFloatingImages';

export function MusicHeroScene() {
  const { fontUrl, heroText, models } = useMusicHeroScene();
  const cameraRef = useRef<THREE.PerspectiveCamera>(null);
  const controlsRef = useRef<ElementRef<typeof OrbitControls>>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [isMobile, setIsMobile] = React.useState(false);

  React.useEffect(() => {
    const checkMobile = () => {
      setIsMobile(window.innerWidth < 1024);
    };
    checkMobile();
    window.addEventListener('resize', checkMobile);
    return () => window.removeEventListener('resize', checkMobile);
  }, []);

  React.useEffect(() => {
    const handleWheelGlobal = (event: WheelEvent) => {
      const controls = controlsRef.current;
      const container = containerRef.current;
      if (!controls || !container) return;

      // Only intercept if the mouse is over this component's container
      if (!container.contains(event.target as Node)) return;

      const distance = controls.getDistance();
      const isScrollingDown = event.deltaY > 0;

      // Check boundaries using the controls' own properties with a larger buffer
      // to account for damping and event timing.
      const atMax = distance >= (controls.maxDistance - 1.0);
      const atMin = distance <= 51.5; 

      if ((atMax && isScrollingDown) || (atMin && !isScrollingDown)) {
        controls.enableZoom = false;
      } else {
        controls.enableZoom = true;
      }
    };

    // Global capture phase ensures we run before the Canvas's native listeners
    window.addEventListener('wheel', handleWheelGlobal, { capture: true });
    return () => window.removeEventListener('wheel', handleWheelGlobal, { capture: true });
  }, []);

  return (
    <div
      ref={containerRef}
      className="w-full h-full bg-[#09090b]"
    >
      <Canvas shadows dpr={[1, 2]}>
        <PerspectiveCamera
          ref={cameraRef}
          makeDefault
          position={isMobile ? [0.08, 10.0, -75.0] : [0.08, 8.68, -63.56]}
          fov={isMobile ? 55 : 45}
        />

        <Sky
          distance={450000}
          sunPosition={[0, -1, 0]}
          turbidity={0.1}
          rayleigh={0.1}
          mieCoefficient={0.005}
          mieDirectionalG={0.7}
        />

        <ambientLight intensity={0.84} />
        <hemisphereLight intensity={0.5} color="white" groundColor="black" />
        <Environment preset="city" />

        <SpotLight
          position={[4.8, 50.8, -9.2]}
          angle={490}
          penumbra={1.0}
          intensity={0.9}
          distance={5620}
          color="#3e3762"
          castShadow
        />

        <OrbitControls
          ref={controlsRef}
          makeDefault
          target={new THREE.Vector3(0, 2, 0)}
          enableDamping
          dampingFactor={0.05}
          minDistance={50}
          maxDistance={60}
          minAzimuthAngle={2.79}
          maxAzimuthAngle={-2.79}
          minPolarAngle={1.292}
          maxPolarAngle={1.536}
        />

        <HeroText
          fontUrl={fontUrl}
          text={heroText}
          position={isMobile ? [0.6, 2.5, -15.0] : [0.6, 2.1, -15.0]}
          rotation={[0, -3.2, 0]}
          scale={isMobile ? 0.7 : 1.03}
        />

        <ArtistFloatingImages />

        <Model
          url={models.room}
          scale={0.3}
          rotation={[0, 0, 0]}
          position={[0, -5, 0]}
        />

        <Model
          url={models.blackCat}
          scale={4.3}
          rotation={[0, 3.2, 0]}
          position={[-3.6, 6.7, -14.7]}
        />

        <Model
          url={models.vinylPlayer}
          scale={15.8}
          rotation={[0, 0, 0]}
          position={[3.5, -5.2, -12.9]}
        />

        <EffectComposer>
          <Bloom
            luminanceThreshold={1}
            mipmapBlur
            intensity={0.80}
            radius={0.4}
          />
        </EffectComposer>
      </Canvas>
    </div>
  );
}
