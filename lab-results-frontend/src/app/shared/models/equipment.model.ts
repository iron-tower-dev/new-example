export interface Equipment {
    id: number;
    equipType: string;
    equipName?: string;
    exclude?: boolean;
    testId?: number;
    dueDate?: Date;
    comments?: string;
    val1?: number;
    val2?: number;
    val3?: number;
    val4?: number;
}

export interface EquipmentSelection {
    id: number;
    equipName: string;
    equipType: string;
    dueDate?: Date;
    isDueSoon: boolean;
    isOverdue: boolean;
    calibrationValue?: number;
    displayText: string;
}

export interface EquipmentCalibration {
    equipmentId: number;
    equipName: string;
    equipType: string;
    calibrationValue?: number;
    dueDate?: Date;
    isValid: boolean;
    validationMessage?: string;
}

export interface EquipmentValidationResult {
    isValid: boolean;
    isDueSoon: boolean;
    isOverdue: boolean;
    message?: string;
    dueDate?: Date;
    daysUntilDue: number;
}

export interface EquipmentFilter {
    equipType?: string;
    testId?: number;
    lubeType?: string;
    excludeInactive?: boolean;
    includeOverdue?: boolean;
}

// Equipment types commonly used in tests
export enum EquipmentType {
    THERMOMETER = 'THERMOMETER',
    TIMER = 'TIMER',
    BAROMETER = 'BAROMETER',
    VISCOMETER = 'VISCOMETER',
    DELETERIOUS = 'DELETERIOUS'
}

// Equipment selection event for components
export interface EquipmentSelectionEvent {
    equipmentId: number;
    equipmentType: string;
    calibrationValue?: number;
    isValid: boolean;
    validationMessage?: string;
}