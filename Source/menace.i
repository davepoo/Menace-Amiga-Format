	IFND	MENACE_I
MENACE_I SET 1

	IFND	HARDWARE_CUSTOM_I
	include 	hardware/custom.i
	ENDC
	IFND	HARDWARE_DMABITS_I
	include		hardware/dmabits.i
	ENDC

; Bit Test dmaconr
; assumes hardware base is in a6
btst_dmaconr	MACRO	
	IIF	\1>7	btst.b	#\1-8,dmaconr(a6)
	IIF	\1<=7	btst.b	#\1,dmaconr+1(a6)
	ENDM

; wait for the blitter to finish
; assumes the hardware base is in a6
blitter_wait MACRO
\@	
	btst.b	#DMAB_BLTDONE-8,dmaconr(a6)
	bne	\@
	ENDM
	
	ENDC	; MENACE_I	
