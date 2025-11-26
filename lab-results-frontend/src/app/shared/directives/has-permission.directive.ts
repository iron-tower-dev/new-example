import { Directive, Input, TemplateRef, ViewContainerRef, inject, OnInit, OnDestroy, effect } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Directive({
    selector: '[appHasPermission]',
    standalone: true
})
export class HasPermissionDirective implements OnInit, OnDestroy {
    private readonly authService = inject(AuthService);
    private readonly templateRef = inject(TemplateRef<any>);
    private readonly viewContainer = inject(ViewContainerRef);
    private readonly destroy$ = new Subject<void>();

    @Input() set appHasPermission(permission: string | { testStandId: number; requiredLevel: string }) {
        this.permission = permission;
        this.updateView();
    }

    @Input() appHasPermissionElse?: TemplateRef<any>;

    private permission: string | { testStandId: number; requiredLevel: string } | null = null;

    ngOnInit(): void {
        // Use effect to react to auth state changes
        effect(() => {
            // Access the signal to trigger the effect
            this.authService.authState();
            this.updateView();
        });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    private updateView(): void {
        const hasPermission = this.checkPermission();

        this.viewContainer.clear();

        if (hasPermission) {
            this.viewContainer.createEmbeddedView(this.templateRef);
        } else if (this.appHasPermissionElse) {
            this.viewContainer.createEmbeddedView(this.appHasPermissionElse);
        }
    }

    private checkPermission(): boolean {
        if (!this.authService.isAuthenticated()) {
            return false;
        }

        if (!this.permission) {
            return true;
        }

        if (typeof this.permission === 'string') {
            // Role-based permission
            const user = this.authService.currentUser();
            switch (this.permission) {
                case 'technician':
                    return this.authService.isTechnician();
                case 'reviewer':
                    return this.authService.isReviewer();
                default:
                    return user?.role === this.permission;
            }
        } else {
            // Test qualification permission
            return this.authService.hasQualification(
                this.permission.testStandId,
                this.permission.requiredLevel
            );
        }
    }
}

// Usage examples:
// *appHasPermission="'technician'" - Show only for technicians
// *appHasPermission="'reviewer'" - Show only for reviewers
// *appHasPermission="{ testStandId: 10, requiredLevel: 'Q/QAG' }" - Show only if user has Q/QAG or higher for test 10