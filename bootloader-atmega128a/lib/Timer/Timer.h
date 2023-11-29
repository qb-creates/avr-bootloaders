#ifndef TIMER_H
#define TIMER_H

#define checkOutputCompareFlag() (ETIFR & _BV(OCF3A))
#define clearTimerFlag() (ETIFR |= _BV(OCF3A))
#define disableTimer() (TCCR3B = 0)

void enableTimer(void);

#endif