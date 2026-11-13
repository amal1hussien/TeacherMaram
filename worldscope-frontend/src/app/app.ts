import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import {
  CountryService,
  Country,
  Currency,
  Favorite
} from './services/country.service';

@Component({selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {

  currentPage: string = 'home';

  // COUNTRIES
  countries: Country[] = [];
  allCountries: Country[] = [];

  // SEARCH
  searchText: string = '';
  selectedRegion: string = '';

  // LOADING AND ERRORS
  loading: boolean = false;
  errorMessage: string = '';

  // FAVORITES
  favorites: Favorite[] = [];
  favoriteCountries: Country[] = [];
  favoritesLoading: boolean = false;
  favoritesMessage: string = '';
  favoritesLoaded: boolean = false;

  // COMPARE
  selectedCountries: Country[] = [];
  comparisonTitle: string = 'My Countries Comparison';
  comparisonMessage: string = '';
  comparisonLoading: boolean = false;
  comparisonSaved: boolean = false;

  constructor(
    private countryService: CountryService
  ) { }

  ngOnInit(): void {
    this.loadCountries();
  }

  // GET CURRENCIES
  // CurrencyData may arrive from the API as a JSON string or an array.
  getCurrencies(country: Country): Currency[] {
    const currencyData = country.currencyData;

    if (!currencyData) {
      return [];
    }

    if (Array.isArray(currencyData)) {
      return currencyData;
    }

    try {
      const parsedData: unknown = JSON.parse(currencyData);

      if (Array.isArray(parsedData)) {
        return parsedData as Currency[];
      }

      return [];
    } catch (error) {
      console.error('Unable to read currency data:', error);
      return [];
    }
  }

  // NAVIGATION
  changePage(page: string): void {
    this.currentPage = page;

    if (page === 'countries' && this.allCountries.length === 0) {
      if (!this.loading) {
        this.loadCountries();
      }
    }

    if (page === 'favorites' && !this.favoritesLoaded) {
      if (this.allCountries.length > 0) {
        this.loadFavorites();
      } else if (!this.loading) {
        this.loadCountries();
      }
    }
  }

  // LOAD ALL COUNTRIES
  loadCountries(): void {
    if (this.loading) return;

    this.loading = true;
    this.errorMessage = '';

    this.countryService.getCountries().subscribe({
      next: (data: Country[]) => {
        this.allCountries = data || [];
        this.countries = [...this.allCountries];

        this.loading = false;

        // Load favorites after countries are available
        if (!this.favoritesLoaded) {
          this.loadFavorites();
        }
      },

      error: (error) => {
        console.error('Countries error:', error);

        this.errorMessage = 'Unable to load countries.';
        this.loading = false;
      }
    });
  }

  // SEARCH

  searchCountries(): void {
    const name = this.searchText.trim().toLowerCase();

    if (!name) {
      this.resetFilters();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.countries = this.allCountries.filter(country =>
      country.name.toLowerCase().includes(name) ||
      country.officialName.toLowerCase().includes(name) ||
      country.countryCode.toLowerCase().includes(name)
    );

    this.loading = false;
  }


  // FILTER BY REGION
  filterByRegion(): void {
    if (!this.selectedRegion) {
      this.countries = [...this.allCountries];
      return;
    }

    this.countries = this.allCountries.filter(country =>
      country.region.toLowerCase() ===
      this.selectedRegion.toLowerCase()
    );
  }

  // RESET FILTERS
  resetFilters(): void {
    this.searchText = '';
    this.selectedRegion = '';
    this.countries = [...this.allCountries];
    this.errorMessage = '';
    this.loading = false;
  }

  // LOAD FAVORITES
  loadFavorites(): void {
    if (this.favoritesLoading) return;

    if (this.allCountries.length === 0) {
      this.favoritesLoaded = false;
      return;
    }

    this.favoritesLoading = true;
    this.favoritesMessage = '';

    this.countryService.getFavorites().subscribe({
      next: (data: Favorite[]) => {
        this.favorites = data || [];

        const countriesMap = new Map(
          this.allCountries.map(country => [
            country.countryCode.toUpperCase(),
            country
          ])
        );

        this.favoriteCountries = this.favorites
          .map(favorite =>
            countriesMap.get(favorite.countryCode.toUpperCase())
          )
          .filter((country): country is Country => country !== undefined);

        this.favoritesLoaded = true;
        this.favoritesLoading = false;
      },

      error: (error) => {
        console.error('Favorites error:', error);

        this.favoritesMessage =
          'Unable to load favorites. Please try again.';

        this.favoritesLoading = false;
        this.favoritesLoaded = false;
      }
    });
  }

  // CHECK IF COUNTRY IS FAVORITE
  isFavorite(country: Country): boolean {
    return this.favorites.some(
      favorite =>
        favorite.countryCode.toUpperCase() ===
        country.countryCode.toUpperCase()
    );
  }

  // ADD TO FAVORITES
  addToFavorites(country: Country): void {
    if (this.isFavorite(country)) {
      alert(country.name + ' is already in Favorites.');
      return;
    }

    this.countryService.addFavorite(
      country.countryCode,
      'My favorite country'
    ).subscribe({
      next: () => {
        // Refresh favorites and get the real ID from API
        this.favoritesLoaded = false;
        this.loadFavorites();

        alert(country.name + ' added to Favorites ❤️');
      },

      error: (error) => {
        console.error('Add favorite error:', error);
        console.error('Server response:', error.error);

        alert(
          error.error?.message ||
          'Unable to add country to Favorites.'
        );
      }
    });
  }

  // GET FAVORITE BY COUNTRY
  getFavorite(country: Country): Favorite | undefined {
    return this.favorites.find(
      favorite =>
        favorite.countryCode.toUpperCase() ===
        country.countryCode.toUpperCase()
    );
  }

  // REMOVE FAVORITE
  removeFavorite(favorite: Favorite): void {
    if (!confirm('Remove this country from Favorites?')) {
      return;
    }

    this.countryService.deleteFavorite(
      favorite.favoriteId
    ).subscribe({
      next: () => {
        this.favoritesLoaded = false;
        this.loadFavorites();
      },

      error: (error) => {
        console.error('Delete favorite error:', error);

        alert('Unable to remove favorite.');
      }
    });
  }

  // CHECK IF COUNTRY IS SELECTED FOR COMPARISON
  isSelectedForCompare(country: Country): boolean {
    return this.selectedCountries.some(
      selected =>
        selected.countryCode === country.countryCode
    );
  }

  // ADD OR REMOVE COUNTRY FROM COMPARISON
  toggleCompare(country: Country): void {
    const index = this.selectedCountries.findIndex(
      selected =>
        selected.countryCode === country.countryCode
    );

    if (index !== -1) {
      this.selectedCountries.splice(index, 1);

      this.comparisonSaved = false;
      this.comparisonMessage = '';

      return;
    }

    if (this.selectedCountries.length >= 2) {
      alert('You can compare only two countries. Remove one first.');
      return;
    }

    this.selectedCountries.push(country);

    this.comparisonSaved = false;
    this.comparisonMessage = '';
  }

  // CREATE COMPARISON
  compareCountries(): void {
    if (this.selectedCountries.length !== 2) {
      this.comparisonMessage =
        'Please select exactly two different countries.';

      return;
    }

    const codes = this.selectedCountries.map(
      country => country.countryCode
    );

    this.comparisonLoading = true;
    this.comparisonMessage = '';
    this.comparisonSaved = false;

    this.countryService.createComparison(
      this.comparisonTitle,
      codes
    ).subscribe({
      next: (response) => {
        console.log('Comparison saved:', response);

        this.comparisonSaved = true;
        this.comparisonLoading = false;

        this.comparisonMessage =
          'Comparison created and saved successfully!';
      },

      error: (error) => {
        console.error('Comparison error:', error);

        this.comparisonLoading = false;
        this.comparisonSaved = false;

        this.comparisonMessage =
          error.error?.message ||
          'Unable to create comparison. Please try again.';
      }
    });
  }

  // CLEAR COMPARISON
  clearComparison(): void {
    this.selectedCountries = [];
    this.comparisonMessage = '';
    this.comparisonSaved = false;
  }
}
