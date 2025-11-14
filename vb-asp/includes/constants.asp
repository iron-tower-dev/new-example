<%<!--constants.asp-->
dim QUOT, CRLF
QUOT=Chr(34)
CRLF=Chr(13) & Chr(10)
const NUMBER_OF_LIMITS=100

'tests
const TAN_COLOR=10
const WATER_KF=20
const SPECT_STD=30
const SPECT_LARGE=40
const VIS40=50
const VIS100=60
const FTIR=70
const FLASH_POINT=80
const FIRE_POINT=90
const TAN_TITRATION=100
const TBN_TITRATION=110
const INSP_FILTER=120
const GREASE_PEN_W=130
const GREASE_DROP_PT=140
const GREASE_LOW_SHEAR=150
const PARTICLE_COUNT=160
const RBOT=170
const FILTER_RES=180
const SOLIDS=190
const WEAR_PART_ANL=200
const FERROGRAPHY=210
const RUST=220
const TFOUT=230
const DEBRIS_ID=240
const DELETERIOUS=250
const XFR_CHLORINE=260
const RHEOMETER=270
const MISC_RES=280
const MISC_CHL=281
const MISC_AMI=282
const MISC_PHE=283
const MISC_D_INCH=284
const MISC_OIL=285
const MISC_THICK=286
'onst MISC_NEUT=287
'const MISC_CONG=288
'const MISC_NIT=289
'const MISC_SUL=290
const GOODNESS=300
const TEMPERATURE=400

const status1="A"		'Awaiting Results
const status2="E"		'Awaiting Microscopic Eval
const status3="C"		'Complete
const status4="R"		'Retest
const status5="X"		'Waiting to be tested
const status6="S"		'

'Limits indices
const LIMIT_PC_5_10=56
const LIMIT_PC_10_15=57
const LIMIT_PC_15_25=58
const LIMIT_PC_25_50=59
const LIMIT_PC_50_100=60
const LIMIT_PC_GT100=61
const LIMIT_PC_NAS=62
const LIMIT_RBOT=63
const LIMIT_FILTER_RES=64
const LIMIT_RUST=65
const LIMIT_TFOUT=66
const LIMIT_RHEOMETER_FIRST=67
const LIMIT_RHEOMETER_LAST=90
const LIMIT_MISC_RES=91
const LIMIT_MISC_CHL=92
const LIMIT_MISC_AMI=93
const LIMIT_MISC_PHE=94
const LIMIT_MISC_D_INCH=95
const LIMIT_MISC_OIL=96
const LIMIT_MISC_THICK=97
const LIMIT_TEMPERATURE=98
%>