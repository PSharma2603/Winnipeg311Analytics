import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface ServiceRequestRecord {
  sr_date: string;
  service_area: string;
  service_request: string;
  ward: string;
  neighbourhood: string;
}

interface AnalyticsItem {
  name: string;
  count: number;
}

interface WardComparison {
  ward: string;
  totalRequests: number;
  topServiceArea: string;
  topServiceAreaCount: number;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {

  serviceRequests: ServiceRequestRecord[] = [];

  wards: string[] = [];
  serviceAreas: string[] = [];

  analytics: AnalyticsItem[] = [];

  selectedWard = '';
  selectedServiceArea = '';

  compareWard1 = '';
  compareWard2 = '';

  comparison1: WardComparison | null = null;
  comparison2: WardComparison | null = null;

  comparisonLoading = false;
  comparisonError = '';

  loading = true;
  errorMessage = '';

  private apiUrl =
    'https://localhost:7216/api/City311';

  constructor(
    private http: HttpClient,
    private changeDetector: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadWards();
    this.loadServiceAreas();
    this.loadServiceRequests();
  }

  loadWards(): void {

    this.http
      .get<string[]>(`${this.apiUrl}/wards`)
      .subscribe({

        next: (data) => {

          this.wards = data;

          this.changeDetector.markForCheck();
        },

        error: (error) => {

          console.error(
            'WARD ERROR:',
            error
          );
        }

      });
  }

  loadServiceAreas(): void {

    this.http
      .get<string[]>(
        `${this.apiUrl}/service-areas`
      )
      .subscribe({

        next: (data) => {

          this.serviceAreas = data;

          this.changeDetector.markForCheck();
        },

        error: (error) => {

          console.error(
            'SERVICE AREA ERROR:',
            error
          );
        }

      });
  }

  loadServiceRequests(): void {

    this.loading = true;
    this.errorMessage = '';

    let url = this.apiUrl;

    const params: string[] = [];

    if (this.selectedWard) {

      params.push(
        `ward=${encodeURIComponent(
          this.selectedWard
        )}`
      );
    }

    if (this.selectedServiceArea) {

      params.push(
        `serviceArea=${encodeURIComponent(
          this.selectedServiceArea
        )}`
      );
    }

    if (params.length > 0) {

      url += '?' + params.join('&');
    }

    this.http
      .get<ServiceRequestRecord[]>(url)
      .subscribe({

        next: (data) => {

          this.serviceRequests = data;

          this.buildAnalytics();

          this.loading = false;

          this.changeDetector.markForCheck();
        },

        error: (error) => {

          console.error(
            '311 ERROR:',
            error
          );

          this.serviceRequests = [];
          this.analytics = [];

          this.errorMessage =
            'Could not load 311 data.';

          this.loading = false;

          this.changeDetector.markForCheck();
        }

      });
  }

  buildAnalytics(): void {

    const counts: {
      [key: string]: number
    } = {};

    for (const request of this.serviceRequests) {

      const area =
        request.service_area ||
        'Unknown';

      if (counts[area]) {
        counts[area]++;
      } else {
        counts[area] = 1;
      }
    }

    this.analytics =
      Object.keys(counts)
        .map(area => ({
          name: area,
          count: counts[area]
        }))
        .sort(
          (a, b) =>
            b.count - a.count
        );
  }

  compareWards(): void {

    this.comparisonError = '';

    this.comparison1 = null;
    this.comparison2 = null;

    if (!this.compareWard1 || !this.compareWard2) {

      this.comparisonError =
        'Please select two wards.';

      return;
    }

    if (this.compareWard1 === this.compareWard2) {

      this.comparisonError =
        'Please select two different wards.';

      return;
    }

    this.comparisonLoading = true;

    const recordsUrl1 =
      `${this.apiUrl}?ward=${encodeURIComponent(
        this.compareWard1
      )}`;

    const recordsUrl2 =
      `${this.apiUrl}?ward=${encodeURIComponent(
        this.compareWard2
      )}`;

    const countUrl1 =
      `${this.apiUrl}/ward-count?ward=${encodeURIComponent(
        this.compareWard1
      )}`;

    const countUrl2 =
      `${this.apiUrl}/ward-count?ward=${encodeURIComponent(
        this.compareWard2
      )}`;

    this.http
      .get<ServiceRequestRecord[]>(recordsUrl1)
      .subscribe({

        next: (data1) => {

          this.http
            .get<number>(countUrl1)
            .subscribe({

              next: (count1) => {

                this.comparison1 =
                  this.createWardComparison(
                    this.compareWard1,
                    data1,
                    count1
                  );

                this.http
                  .get<ServiceRequestRecord[]>(recordsUrl2)
                  .subscribe({

                    next: (data2) => {

                      this.http
                        .get<number>(countUrl2)
                        .subscribe({

                          next: (count2) => {

                            this.comparison2 =
                              this.createWardComparison(
                                this.compareWard2,
                                data2,
                                count2
                              );

                            this.comparisonLoading = false;

                            this.changeDetector.markForCheck();
                          },

                          error: (error) => {

                            console.error(
                              'WARD 2 COUNT ERROR:',
                              error
                            );

                            this.comparisonError =
                              'Could not load ward totals.';

                            this.comparisonLoading = false;

                            this.changeDetector.markForCheck();
                          }

                        });
                    },

                    error: (error) => {

                      console.error(
                        'WARD 2 RECORD ERROR:',
                        error
                      );

                      this.comparisonError =
                        'Could not compare the selected wards.';

                      this.comparisonLoading = false;

                      this.changeDetector.markForCheck();
                    }

                  });
              },

              error: (error) => {

                console.error(
                  'WARD 1 COUNT ERROR:',
                  error
                );

                this.comparisonError =
                  'Could not load ward totals.';

                this.comparisonLoading = false;

                this.changeDetector.markForCheck();
              }

            });
        },

        error: (error) => {

          console.error(
            'WARD 1 RECORD ERROR:',
            error
          );

          this.comparisonError =
            'Could not compare the selected wards.';

          this.comparisonLoading = false;

          this.changeDetector.markForCheck();
        }

      });
  }

  createWardComparison(
    ward: string,
    records: ServiceRequestRecord[],
    totalRequests: number
  ): WardComparison {

    const counts: {
      [key: string]: number
    } = {};

    for (const record of records) {

      const area =
        record.service_area ||
        'Unknown';

      if (counts[area]) {
        counts[area]++;
      } else {
        counts[area] = 1;
      }
    }

    let topServiceArea = 'N/A';
    let topServiceAreaCount = 0;

    for (const area of Object.keys(counts)) {

      if (
        counts[area] >
        topServiceAreaCount
      ) {

        topServiceArea = area;

        topServiceAreaCount =
          counts[area];
      }
    }

    return {
      ward: ward,
      totalRequests: totalRequests,
      topServiceArea: topServiceArea,
      topServiceAreaCount: topServiceAreaCount
    };
  }

  clearFilters(): void {

    this.selectedWard = '';

    this.selectedServiceArea = '';

    this.loadServiceRequests();
  }
}