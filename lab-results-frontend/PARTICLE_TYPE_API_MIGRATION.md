# Particle Type API Migration

## Overview

Particle type definitions are now loaded from the database via the API instead of being hardcoded in the frontend. This ensures consistency with the legacy VB application and allows particle types to be managed centrally in the database.

## Changes Made

### Backend (Already in place)
- ✅ `ParticleTypeDefinition` table in SQL Server database
- ✅ `ParticleTypeDefinitionDto` in `LabResultsApi/DTOs/`
- ✅ `ILookupService` interface with `GetParticleTypeDefinitionsAsync()` method
- ✅ `LookupService` implementation with caching
- ✅ API endpoint at `/api/lookups/particle-types`

### Frontend Updates

#### 1. Service Layer
- **`test.service.ts`**: Updated to delegate to `LookupService` instead of calling non-existent particle-analysis endpoints
  - `getParticleTypes()` now uses `LookupService.getParticleTypeDefinitions()`
  - `getSubTypeCategories()` now uses `LookupService.getParticleSubTypeCategories()`

#### 2. Utility Functions
- **New file: `shared/utils/particle-type.util.ts`**
  - `categorizeParticleType()`: Maps particle type names to UI categories (wear, oxide, contaminant, other)
  - `addCategoryToParticleType()`: Converts API DTO to extended type with category
  - `getParticleTypesByCategory()`: Filters particle types by category
  - `getActiveParticleTypes()`: Filters and sorts active particle types
  - `getParticleTypeById()`: Finds particle type by ID

#### 3. Constants File
- **`shared/constants/ferrography-particle-types.ts`**: Deprecated
  - Added `@deprecated` JSDoc comment
  - File kept for backwards compatibility but should not be used
  - All new code should fetch from API

#### 4. Component Updates
- **`ferrography-test-entry.component.ts`**: Updated to use utility functions
  - Removed local `categorizeParticleType()` method
  - Uses `addCategoryToParticleType()` from util when loading data
  - Imports from `particle-type.util.ts`

## API Usage

### Fetching Particle Types

```typescript
import { LookupService } from '@shared/services/lookup.service';
import { addCategoryToParticleType } from '@shared/utils/particle-type.util';

// Inject the service
constructor(private lookupService: LookupService) {}

// Fetch particle types
this.lookupService.getParticleTypeDefinitions().subscribe(types => {
  // Convert to types with categories if needed
  const typesWithCategories = types.map(pt => addCategoryToParticleType(pt));
  
  // Use the particle types
  this.particleTypes.set(typesWithCategories);
});
```

### Data Structure

#### API Response (`ParticleTypeDefinitionDto`)
```typescript
{
  id: number;
  type: string;          // e.g. "Cutting Wear", "Red Oxides"
  description: string;
  image1: string;        // Image filename or path
  image2: string;        // Image filename or path
  active: string;        // 'Y' or 'N' (database uses char(1))
  sortOrder: number;
}
```

#### Extended Type with Category (`ParticleTypeWithCategory`)
```typescript
{
  ...ParticleTypeDefinitionDto,
  category: 'wear' | 'oxide' | 'contaminant' | 'other'
}
```

## Category Mapping

The `categorizeParticleType()` function maps particle type names to categories:

| Category | Keywords |
|----------|----------|
| **wear** | cutting, sliding, fatigue, spherical, nonferrous, wear |
| **oxide** | oxide, rust, corrosion |
| **contaminant** | contaminant, fiber, dirt, sand |
| **other** | Default for unmatched types |

## Database Schema

The `ParticleTypeDefinition` table contains:

```sql
CREATE TABLE ParticleTypeDefinition (
    ID INT IDENTITY PRIMARY KEY,
    [Type] NVARCHAR(50) NOT NULL,
    [Description] NVARCHAR(500),
    Image1 NVARCHAR(50),
    Image2 NVARCHAR(50),
    Active CHAR(1) DEFAULT 'Y',
    SortOrder INT
);
```

## Caching

The backend `LookupService` implements memory caching with 1-hour expiration for particle type data. The cache can be manually refreshed using:

```
POST /api/lookups/particle-types/refresh-cache
```

## Testing

When testing components that use particle types:

```typescript
// Mock the LookupService in tests
const mockLookupService = jasmine.createSpyObj('LookupService', [
  'getParticleTypeDefinitions',
  'getParticleSubTypeCategories'
]);

// Provide mock data
mockLookupService.getParticleTypeDefinitions.and.returnValue(of([
  {
    id: 1,
    type: 'Cutting Wear',
    description: 'Normal machining wear',
    image1: '/assets/particles/cutting-wear-1.jpg',
    image2: '/assets/particles/cutting-wear-2.jpg',
    active: 'Y',
    sortOrder: 1
  }
]));
```

## Migration Notes

### For Existing Code
1. Replace imports from `ferrography-particle-types.ts` with API calls
2. Use `LookupService.getParticleTypeDefinitions()` instead of `FERROGRAPHY_PARTICLE_TYPES`
3. Apply `addCategoryToParticleType()` if category field is needed
4. Update tests to mock `LookupService` instead of importing constants

### For New Code
1. Always fetch particle types from the API via `LookupService`
2. Use utility functions from `particle-type.util.ts` for common operations
3. Do NOT hardcode particle type data

## Benefits

1. **Single Source of Truth**: Database is the authoritative source
2. **Consistency**: Same data in Angular app and legacy VB app
3. **Maintainability**: Update particle types in one place (database)
4. **Caching**: Backend caching improves performance
5. **Flexibility**: Add/modify particle types without frontend deployment

## Related Files

- Backend:
  - `LabResultsApi/Models/ParticleTypeDefinition.cs`
  - `LabResultsApi/DTOs/ParticleTypeDefinitionDto.cs`
  - `LabResultsApi/Services/ILookupService.cs`
  - `LabResultsApi/Services/LookupService.cs`
  - `LabResultsApi/Endpoints/LookupEndpoints.cs`
  - `db-tables/ParticleTypeDefinition.sql`

- Frontend:
  - `lab-results-frontend/src/app/shared/services/lookup.service.ts`
  - `lab-results-frontend/src/app/shared/services/test.service.ts`
  - `lab-results-frontend/src/app/shared/utils/particle-type.util.ts`
  - `lab-results-frontend/src/app/shared/constants/ferrography-particle-types.ts` (deprecated)
  - `lab-results-frontend/src/app/features/test-entry/components/ferrography-test-entry/`
