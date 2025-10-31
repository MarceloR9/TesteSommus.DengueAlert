import { TestBed } from '@angular/core/testing';

import { DengueService } from './dengue.service';

describe('DengueService', () => {
  let service: DengueService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DengueService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
