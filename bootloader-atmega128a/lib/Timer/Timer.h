#ifndef TIMER_H
#define TIMER_H

class TIMER0
{
	public:
		static void configure();
		static void adjustSpeed(int ocr0);
};

class TIMER1A
{
	public:
		static void configure();
		static void adjustSpeed(int ocr1a);
};

class TIMER3A
{
	public:
		static void configure();
		static void adjustSpeed(int ocr3a);
};
#endif