import { Injectable } from '@angular/core';
import { PreloadingStrategy, Route } from '@angular/router';
import { Observable, of } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class SelectivePreloadStrategy implements PreloadingStrategy {
    preload(route: Route, load: () => Observable<any>): Observable<any> {
        // Preload routes that are marked for preloading
        if (route.data && route.data['preload']) {
            console.log('Preloading: ' + route.path);
            return load();
        }

        // Don't preload routes that are not marked
        return of(null);
    }
}