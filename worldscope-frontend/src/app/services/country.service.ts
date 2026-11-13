import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Currency {
  code?: string;
  name?: string;
  symbol?: string;
}

export interface Country {
  countryCode: string;
  name: string;
  officialName: string;
  capital: string;
  region: string;
  subregion: string;
  population: number;
  area: number;
  flagUrl: string;
  // The backend stores CurrencyData as JSON text.
  currencyData?: string | Currency[] | null;
}

export interface Favorite {
  favoriteId: number;
  countryCode: string;
  notes: string;
  createdAt?: string;
}

export interface Comparison {
  comparisonId: number;
  title: string;
  createdAt?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CountryService {

  private apiUrl = 'https://localhost:7111/api/countries';
  private favoritesUrl = 'https://localhost:7111/api/Favorites';
  private comparisonsUrl = 'https://localhost:7111/api/Comparisons';

  constructor(private http: HttpClient) { }

  // COUNTRIES

  getCountries(): Observable<Country[]> {
    return this.http.get<Country[]>(this.apiUrl);
  }

  searchCountries(name: string): Observable<Country[]> {
    return this.http.get<Country[]>(
      `${this.apiUrl}/search?name=${encodeURIComponent(name)}`
    );
  }

  getCountriesByRegion(region: string): Observable<Country[]> {
    return this.http.get<Country[]>(
      `${this.apiUrl}/region/${encodeURIComponent(region)}`
    );
  }

  getCountryByCode(code: string): Observable<Country> {
    return this.http.get<Country>(`${this.apiUrl}/${code}`);
  }


  // FAVORITES

  getFavorites(): Observable<Favorite[]> {
    return this.http.get<Favorite[]>(this.favoritesUrl);
  }

  addFavorite(countryCode: string, notes: string): Observable<any> {
    return this.http.post(this.favoritesUrl, {
      countryCode: countryCode,
      notes: notes
    });
  }

  deleteFavorite(id: number): Observable<any> {
    return this.http.delete(`${this.favoritesUrl}/${id}`);
  }

  updateFavoriteNotes(id: number, notes: string): Observable<any> {
    return this.http.put(
      `${this.favoritesUrl}/${id}/notes`,
      { notes: notes }
    );
  }


  // COMPARISONS

  createComparison(
    title: string,
    countryCodes: string[]
  ): Observable<any> {

    return this.http.post(this.comparisonsUrl, {
      title: title,
      countryCodes: countryCodes
    });
  }

  getComparison(id: number): Observable<any> {
    return this.http.get(`${this.comparisonsUrl}/${id}`);
  }

}
