# fm-index

FM-index implementation based on wavelet tree.

Some notes:
* The smaller the alphabet, the smaller the space occupancy
* Case-insensitive search, both text and pattern converted to lowercase
* Text and pattern are converted to the derived alphabet
* In Locate operation we examine all alphabet characters to determine char for LF-mapping operation
* Wavelet tree node with alphabet range 2 is a leaf
* Wavelet tree node with anchor character has alphabet range 3

<hr />

<p align="center">
<img src="images/fm-index.png" />
</p>

<hr />

<img src="images/table_1.png" />

<img src="images/table_2.png" />
