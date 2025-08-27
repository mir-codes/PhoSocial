import { trigger, transition, style, animate, query, stagger } from '@angular/animations';

export const fadeInUp = trigger('fadeInUp', [
  transition(':enter', [
    style({ 
      opacity: 0, 
      transform: 'translateY(20px)' 
    }),
    animate('0.3s ease-out', 
      style({ 
        opacity: 1, 
        transform: 'translateY(0)' 
      })
    )
  ])
]);

export const listAnimation = trigger('listAnimation', [
  transition('* <=> *', [
    query(':enter', [
      style({ 
        opacity: 0, 
        transform: 'translateY(20px)' 
      }),
      stagger('50ms', [
        animate('0.3s ease-out', 
          style({ 
            opacity: 1, 
            transform: 'translateY(0)' 
          })
        )
      ])
    ], { optional: true })
  ])
]);

export const slideInRight = trigger('slideInRight', [
  transition(':enter', [
    style({ 
      transform: 'translateX(100%)', 
      opacity: 0 
    }),
    animate('0.3s ease-out', 
      style({ 
        transform: 'translateX(0)', 
        opacity: 1 
      })
    )
  ]),
  transition(':leave', [
    animate('0.3s ease-in', 
      style({ 
        transform: 'translateX(100%)', 
        opacity: 0 
      })
    )
  ])
]);

export const expandCollapse = trigger('expandCollapse', [
  transition(':enter', [
    style({ 
      height: '0', 
      opacity: 0 
    }),
    animate('0.2s ease-out', 
      style({ 
        height: '*', 
        opacity: 1 
      })
    )
  ]),
  transition(':leave', [
    animate('0.2s ease-in', 
      style({ 
        height: '0', 
        opacity: 0 
      })
    )
  ])
]);
