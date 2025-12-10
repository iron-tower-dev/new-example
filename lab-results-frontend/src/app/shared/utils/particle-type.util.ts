/**
 * Particle Type Utilities
 * 
 * Utility functions for working with particle type data from the API
 */

import { ParticleTypeDefinitionDto } from '../services/lookup.service';

/**
 * Particle type categories for organization and filtering
 */
export type ParticleCategory = 'wear' | 'oxide' | 'contaminant' | 'other';

/**
 * Extended particle type definition with category
 */
export interface ParticleTypeWithCategory extends ParticleTypeDefinitionDto {
    category: ParticleCategory;
}

/**
 * Categorize a particle type based on its name
 * This maps the database particle type names to UI categories
 */
export function categorizeParticleType(typeName: string): ParticleCategory {
    const normalized = typeName.toLowerCase().trim();
    
    // Wear particles
    if (normalized.includes('cutting') || 
        normalized.includes('sliding') || 
        normalized.includes('fatigue') || 
        normalized.includes('spherical') ||
        normalized.includes('nonferrous') ||
        normalized.includes('wear')) {
        return 'wear';
    }
    
    // Oxide particles
    if (normalized.includes('oxide') || 
        normalized.includes('rust') ||
        normalized.includes('corrosion')) {
        return 'oxide';
    }
    
    // Contaminants
    if (normalized.includes('contaminant') || 
        normalized.includes('fiber') ||
        normalized.includes('dirt') ||
        normalized.includes('sand')) {
        return 'contaminant';
    }
    
    // Default to 'other'
    return 'other';
}

/**
 * Convert API particle type DTO to particle type with category
 */
export function addCategoryToParticleType(dto: ParticleTypeDefinitionDto): ParticleTypeWithCategory {
    return {
        ...dto,
        category: categorizeParticleType(dto.type || '')
    };
}

/**
 * Get particle types by category
 */
export function getParticleTypesByCategory(
    particleTypes: ParticleTypeWithCategory[], 
    category: ParticleCategory
): ParticleTypeWithCategory[] {
    return particleTypes.filter(pt => pt.category === category);
}

/**
 * Get active particle types sorted by sort order
 */
export function getActiveParticleTypes(particleTypes: ParticleTypeWithCategory[]): ParticleTypeWithCategory[] {
    return particleTypes
        .filter(pt => pt.active === 'Y' || pt.active === true || pt.active === undefined)
        .sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0));
}

/**
 * Get particle type by ID
 */
export function getParticleTypeById(
    particleTypes: ParticleTypeWithCategory[], 
    id: number
): ParticleTypeWithCategory | undefined {
    return particleTypes.find(pt => pt.id === id);
}
