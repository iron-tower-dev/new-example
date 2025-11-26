/**
 * Ferrography-specific particle type definitions
 * 
 * This file contains the 10 standard ferrography particle types used for
 * microscopic wear particle analysis in lubrication systems.
 */

import { ParticleTypeDefinition } from '../models/particle-analysis.model';

/**
 * Standard ferrography particle types with descriptions and categorization
 */
export const FERROGRAPHY_PARTICLE_TYPES: ParticleTypeDefinition[] = [
    {
        id: 1,
        type: 'Cutting Wear',
        description: 'Normal machining wear particles with sharp edges and metallic appearance',
        category: 'wear',
        image1: '/assets/particles/cutting-wear-1.jpg',
        image2: '/assets/particles/cutting-wear-2.jpg',
        active: true,
        sortOrder: 1
    },
    {
        id: 2,
        type: 'Sliding Wear',
        description: 'Adhesive wear particles from sliding contact surfaces',
        category: 'wear',
        image1: '/assets/particles/sliding-wear-1.jpg',
        image2: '/assets/particles/sliding-wear-2.jpg',
        active: true,
        sortOrder: 2
    },
    {
        id: 3,
        type: 'Severe Sliding',
        description: 'Large sliding wear particles indicating severe adhesive wear conditions',
        category: 'wear',
        image1: '/assets/particles/severe-sliding-1.jpg',
        image2: '/assets/particles/severe-sliding-2.jpg',
        active: true,
        sortOrder: 3
    },
    {
        id: 4,
        type: 'Fatigue Particles',
        description: 'Particles from surface fatigue, spalling, and rolling contact wear',
        category: 'wear',
        image1: '/assets/particles/fatigue-1.jpg',
        image2: '/assets/particles/fatigue-2.jpg',
        active: true,
        sortOrder: 4
    },
    {
        id: 5,
        type: 'Spherical Particles',
        description: 'Rolling contact fatigue particles with spherical or rounded morphology',
        category: 'wear',
        image1: '/assets/particles/spherical-1.jpg',
        image2: '/assets/particles/spherical-2.jpg',
        active: true,
        sortOrder: 5
    },
    {
        id: 6,
        type: 'Dark Metallic Oxides',
        description: 'Oxidized iron particles from wear surfaces exposed to high temperatures',
        category: 'oxide',
        image1: '/assets/particles/dark-oxide-1.jpg',
        image2: '/assets/particles/dark-oxide-2.jpg',
        active: true,
        sortOrder: 6
    },
    {
        id: 7,
        type: 'Red Oxides',
        description: 'Rust particles from corrosion processes and water contamination',
        category: 'oxide',
        image1: '/assets/particles/red-oxide-1.jpg',
        image2: '/assets/particles/red-oxide-2.jpg',
        active: true,
        sortOrder: 7
    },
    {
        id: 8,
        type: 'Nonferrous Particles',
        description: 'Copper, aluminum, bronze, and other nonferrous metal wear particles',
        category: 'wear',
        image1: '/assets/particles/nonferrous-1.jpg',
        image2: '/assets/particles/nonferrous-2.jpg',
        active: true,
        sortOrder: 8
    },
    {
        id: 9,
        type: 'Fibers',
        description: 'Organic or synthetic fibers from seals, gaskets, and filtration systems',
        category: 'contaminant',
        image1: '/assets/particles/fibers-1.jpg',
        image2: '/assets/particles/fibers-2.jpg',
        active: true,
        sortOrder: 9
    },
    {
        id: 10,
        type: 'Contaminants',
        description: 'External contamination particles including dirt, sand, and environmental debris',
        category: 'contaminant',
        image1: '/assets/particles/contaminants-1.jpg',
        image2: '/assets/particles/contaminants-2.jpg',
        active: true,
        sortOrder: 10
    }
];

/**
 * Particle categories for filtering and organization
 */
export const PARTICLE_CATEGORIES = {
    WEAR: 'wear',
    OXIDE: 'oxide',
    CONTAMINANT: 'contaminant',
    OTHER: 'other'
} as const;

/**
 * Get particle types by category
 */
export function getParticleTypesByCategory(category: string): ParticleTypeDefinition[] {
    return FERROGRAPHY_PARTICLE_TYPES.filter(pt => pt.category === category && pt.active);
}

/**
 * Get particle type by ID
 */
export function getParticleTypeById(id: number): ParticleTypeDefinition | undefined {
    return FERROGRAPHY_PARTICLE_TYPES.find(pt => pt.id === id);
}

/**
 * Get all active particle types sorted by sort order
 */
export function getActiveParticleTypes(): ParticleTypeDefinition[] {
    return FERROGRAPHY_PARTICLE_TYPES
        .filter(pt => pt.active)
        .sort((a, b) => a.sortOrder - b.sortOrder);
}