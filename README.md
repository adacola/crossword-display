# crossword-display

クロスワードの単語を一定時間ごとに表示するツールです。

## ビルド方法

Linux版をビルドする際は、以下のスクリプトでdockerコンテナ内でビルドする方法がおすすめです。

```
$ ./build-scripts/build-single-docker.sh
```

その他の環境用のビルドには `build-single.sh` を使ってください。

## Usage

```
$ ./crossword-display --help                                                                                         USAGE: crossword-display [--help] [--wait <ms>]

OPTIONS:

    --wait <ms>           待機時間をミリ秒単位で指定します。省略時は4000
    --help                display this list of options.
```

## 使用方法

ツールを実行すると、入力受付状態になります。

例えば「？ち？？ま？」や「a??e?」など、不明な部分を？にして入力すると、該当する単語を言って時間ごとに次々に表示します。  
あとは表示された単語を使って各自何かしてください。

「next」または「n」と入力すると、一定時間を待たずにすぐ次の単語を表示します。  
「quit」または「q」と入力するとツールが終了します。
