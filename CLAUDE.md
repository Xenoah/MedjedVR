# CLAUDE.md

このファイルは、このリポジトリで作業する Claude Code（および開発者）のための作業ガイドです。
README.md に書かれた仕様（特に「実装予定」機能）を、Basis フレームワークの流儀に沿って実装していくためのプロセスをまとめています。

- プロジェクトの背景・思想: [PHILOSOPHY.md](./PHILOSOPHY.md)
- コーディング/レビュー規約（**作業前に必読**）: [STYLE.md](./STYLE.md)
- CI/ビルド: [CI.md](./CI.md)
- 商標・ライセンス: [TRADEMARK.md](./TRADEMARK.md) / [LICENSE](./LICENSE)

---

## 1. プロジェクトの全体像

XenuyuVR は、OSS の VRSNS フレームワーク **Basis** をフォークし、「チル・ゲーム対応 VRSNS」として独自機能を追加していくプロジェクトです。

### 重要な構造上の事実

- **ゲームエンジン**: Unity `6000.5.1f1`、URP、IL2CPP、OpenXR / OpenVR（SteamVR）。
- **Unity プロジェクト本体**は `Basis/` フォルダー。Unity Hub ではリポジトリのルートではなく **`Basis/` を開く**。
- **コードの本体は `Basis/Assets` ではなく `Basis/Packages/` のローカル UPM パッケージ群**にある。
  `Basis/Assets/Basis` には `Settings` と `link.xml` しか無い。新機能は原則として「パッケージ」として足すか、既存パッケージを拡張する。
- 起動シーン: `Basis/Packages/com.basis.framework/Scenes/initialization.unity`（Play 前にこれを開く）。
- パッケージ依存は `Basis/Packages/manifest.json` / `packages-lock.json` で管理。

### コードが置かれている主要パッケージ（抜粋）

| パッケージ | 役割 |
|---|---|
| `com.basis.framework` | 中核。起動・avatar・loading・ネットワーク基盤、`initialization.unity` |
| `com.basis.framework.editor` | Editor 拡張・ビルド系 |
| `com.basis.eventdriver` | フレーム毎処理を束ねる `BasisEventDriver` |
| `com.basis.sdk` | ワールド/アバター制作 SDK |
| `com.basis.server` / `com.basis.provider.servers` | サーバー接続 |
| `com.basis.settings` | 設定 |
| `com.basis.mediaplayer` / `com.basis.integration.ytdlp` | 動画プレーヤー基盤 |
| `com.basis.mediapipe` / `com.basis.visualtrackers` | フェイス/ボディトラッキング |
| `com.basis.openlipsync` | リップシンク |
| `com.basis.examples`（`Mirror`/`Interactable`/`Highlight`） | ミラー等のサンプル実装 |
| `dev.hai-vr.basis.ndmf` / `dev.hai-vr.basis.comms` | アバター加工 (NDMF)・コミュニケーション |
| `com.basis.openxr` / `com.basis.openvr` / `com.steam.steamvr` | XR ランタイム |

---

## 2. 作業の大原則（Definition of Done）

STYLE.md が正式な規約。ここでは Claude が毎回守る要点だけ再掲する。

1. **既存の面を先に再利用する。** 新しい event/hook/driver/registry を足す前に、既存の callback（`StartDevice`/`StopDevice`、`StaticCurrentMode`、`BasisEventDriver`、ネットワークの `Try*` helper 等）で足りないか確認する。足りない場合は PR 説明に「既存の何が、なぜ足りないか」を書く。
2. **フレーム毎処理は単独の `Update`/`LateUpdate`/`FixedUpdate` を足さず、`BasisEventDriver` 経由**にする。
3. **カメラは `BasisLocalCameraDriver`**、**ログは `LogTag` 付き `BasisDebug`**、**アセット読込は Addressables**（新規 `Resources.Load` 禁止）。
4. **シーン全体探索（`FindObjectOfType`/`GameObject.Find`/`transform.Find`）禁止。** 依存は明示的に繋ぐ。
5. **`GetComponent` ではなく `TryGetComponent`**、結果はキャッシュ。失敗が想定される処理は **Try パターン**。
6. **ホットパスでアロケーションしない**（`new`/LINQ/文字列補間/boxing/インターフェース型 `foreach`）。文字列は一度だけ hash 化。
7. **機能は自然な所有者に置く**（マイク系は UI ではなくマイク driver へ）。
8. **境界で検証する**（壊れたデータはシステムに入る地点で直す。各呼び出し地点に null チェックを撒かない）。
9. **フォーマット**: `.editorconfig` + CSharpier（`.csharpierignore`）。コメントは薄く、「なぜ」だけ書く。public field は OK（飾りの `{ get; set; }` 不要）。
10. **コミット/PR**: メッセージは英語の conventional 形式（`feat:`/`fix:`/`docs:` …、既存 git log 参照）。ブランチは `developer`（= デフォルト）。

> Claude への運用注意: ビルド確認は Unity Editor / Windows Player が必要で、この環境では実行できないことが多い。**コードを書いたら「未検証」であることを必ず明示**し、ユーザーに Editor / Player での確認手順を渡す。`Builds/`・`Artifacts/`・Unity ログはコミットしない。

---

## 3. 機能を追加するときの標準フロー（全機能共通フェーズ）

README の各「実装予定」機能は、以下の 7 フェーズで進める。各フェーズの成果物を PR 説明や本ファイルの作業ログに残す。

### フェーズ 0 — 調査・設計合意
- 対象機能の **既存資産**を grep で洗い出す（例: ミラーなら `com.basis.examples/Mirror`、動画なら `com.basis.mediaplayer`）。
- VR / Desktop / Mobile(Quest) の**入力モード差**と、ネットワーク同期の要否を整理。
- 「既存パッケージの拡張」か「新規パッケージ（命名は後述）」かを決める。
- 設計メモ（データの所有者・同期方式・失敗時挙動）を 1 つにまとめ、**実装前にユーザーへ提示して合意**を取る。
- 出力: 設計メモ + 影響パッケージ一覧。

### フェーズ 1 — スキャフォールド
- 新規パッケージなら `Basis/Packages/com.xenuyu.<feature>/` を作成（`package.json`/asmdef/`Runtime`/`Editor`/`README.md`）。命名は §5 参照。
- 既存拡張なら、対象パッケージ内の自然な場所にファイルを置く。
- asmdef の依存を最小限に張る（framework / eventdriver / 必要 XR のみ）。
- 出力: ビルドの通る空コンポーネント + meta。

### フェーズ 2 — ローカル（非同期）コア実装
- まず**自分一人のローカル挙動**を完成させる（ネットワークなし）。
- フレーム毎処理は `BasisEventDriver`、入力は InputSystem / XR Hands、カメラは `BasisLocalCameraDriver`。
- 失敗経路は `Try*`、ログは `BasisDebug` + 専用 `LogTag`。
- 出力: Desktop で動くローカル機能 + 手動確認手順。

### フェーズ 3 — XR / 入力モード対応
- VR（コントローラー/ハンドトラッキング）、Desktop、Mobile touch の 3 経路を通す。
- 実行中の **Desktop ↔ VR 切り替え**で teardown/再初期化が壊れないか確認（regression 多発ポイント）。
- 出力: 各入力モードの確認結果（VR は headset 名を記録）。

### フェーズ 4 — ネットワーク同期（必要な機能のみ）
- 同期が要る機能（ペンの描画、動画の再生位置、撫で音、姿勢など）は、Basis のネットワーク／所有権 API を使う。
- **所有権は `TryGetOwnershipId(out …)` 等の Try 系**で扱い、後入室者への状態復元（late-join）を設計する。
- 帯域を意識（毎フレーム full state を送らない。差分/イベント駆動）。
- 出力: 2 クライアント以上での同期確認手順。

### フェーズ 5 — UI / ローカライズ / 設定
- UI は既存の UIElements / メニュー流儀に合わせる。新規探索処理を作らない。
- 文言は**日本語を基準**にしつつ、ローカライズ機構があればそれに乗せる（README の方針: 日本語ユーザーに使いやすく）。
- ユーザー設定が要るものは `com.basis.settings` に寄せる。
- 出力: メニュー導線 + 設定項目。

### フェーズ 6 — 検証・最適化・ドキュメント
- STYLE.md の PR チェックリストを上から全部埋める（N/A は理由を書く）。
- ホットパスのアロケーション/探索/文字列 hash を点検。Job 化できる処理は検討、できなければ理由を Notes に。
- Windows（基準）+ 可能なら Quest でビルド確認。
- パッケージ `README.md` と本ファイルの作業ログを更新。
- 出力: PR（Summary に「何を・なぜ」、テスト表、未検証項目を明記）。

---

## 4. 機能別の実装ノート（README「実装予定」）

各機能の「土台になる既存資産」と「フェーズ上の勘所」。詳細設計はフェーズ 0 で確定する。

| 機能 | 土台になりそうな既存資産 | 勘所（同期/入力など） |
|---|---|---|
| **ペン** | `com.basis.examples/Interactable`・`Highlight`、SDK の grab 系 | 線をネットワーク同期（フェーズ4必須）。描画点は差分送信。消去/Undo とパフォーマンス（線分の上限）。 |
| **使いやすい動画プレーヤー** | `com.basis.mediaplayer`、`com.basis.integration.ytdlp`、`com.basis.integration.audiolink` | URL 解決は ytdlp 連携。**再生位置・再生/停止をオーナー同期**。失敗 URL は §README の「失敗リソースをスキップ」方針に合わせる。 |
| **カラオケ向け遅延調整システム** | mediaplayer の音声経路、AudioLink | 各クライアントで個別に音声/映像オフセットを調整できる**ローカル設定**。同期するのは曲・再生位置、オフセットはローカル（フェーズ5 設定）。 |
| **フェイスミラー** | `com.basis.examples/Mirror`、`com.basis.mediapipe`、`com.basis.openlipsync` | 自分のアバター/表情を映す。ミラーは描画コストが高いので**オン/オフとレイヤー制御**。同期不要（ローカル）。 |
| **姿勢変更ツール** | framework の avatar/IK、`com.basis.visualtrackers` | 座り/寝そべり等のポーズ適用。IK と calibration を壊さない。他者に見える姿勢はネットワーク同期。 |
| **撫で音** | Interactable（接触判定）、audio 経路 | 接触トリガで SE 再生。**近接プレイヤーへ音を同期**しつつ spam 防止（クールダウン）。アロケーションに注意。 |
| **VRM アバター変換ツール** | `dev.hai-vr.basis.ndmf`、`com.basis.sdk` | VRM → Basis avatar 変換。**Editor 専用ツール**（runtime で実行しない／runtime 呼び出しは明確にエラー）。NDMF パイプラインに乗せる。 |
| **チルワールド** | `com.basis.sdk`、`com.basis.examples`、URP/volumetric fog | ワールド制作。SDK のワールドビルド導線に従う。ライティング/負荷の最適化。 |
| **ゲームワールド** | `com.basis.pooltable`（ネットワークゲーム実例）、SDK | ネットワークゲームの所有権・状態同期・late-join をプールテーブル実装を参考に設計。 |

---

## 5. 命名・配置の規約（このプロジェクト固有）

- **独自機能の新規パッケージは `com.xenuyu.<feature>`** に置く（Basis 本体の `com.basis.*` と混ぜない＝アップストリーム追従を容易にするため）。
  - 例: `com.xenuyu.pen`, `com.xenuyu.karaoke`, `com.xenuyu.facemirror`。
- **Basis 既存パッケージの改変は最小限**にし、改変した場合は PR 説明とコミットで明示（アップストリームとの差分管理）。
- ブランド名は **XenuyuVR**。Basis の商標方針は [TRADEMARK.md](./TRADEMARK.md) に従う（「Built with Basis」表記は維持）。
- アセット/コードのコメント・UI 文言は日本語可だが、**識別子・コミット・PR は英語**。

---

## 6. ビルド & 動作確認（要点）

詳細は README §開発者向けセットアップ / §Windows ビルド、CI は CI.md。

- Editor 起動前に `initialization.unity` を開く。
- VR を無効化して起動: `--disable-OpenVRLoader --disable-OpenXRLoader` / 強制: `--force-OpenXRLoader --force-OpenVRLoader`。
- UI・サーバー接続・ローカライズ・ビルド設定を変えたら **Editor Play と Windows Player の両方**で確認。
- ヘッドレス/CI ビルドは Addressables もビルド対象に含める。

---

## 7. このリポジトリの改善点（修正すべきポイント）

作業を進めるうえで先に潰しておくべき／注意すべき点。優先度順。

1. **【欠落】`CONTRIBUTING.md` が無い。** STYLE.md 冒頭・PR テンプレートが `CONTRIBUTING.md` を参照しているのにファイルが存在しない（リンク切れ）。issue 起票〜PR〜ビルドの流れを書いた `CONTRIBUTING.md` を追加するか、参照を STYLE.md に統合する。
2. **【ロードマップ未定義】README の「実装予定」が箇条書きのみ**で、優先順位・依存関係・完了基準が無い。本ファイル §4 を起点に、各機能の優先度とマイルストーンを決める（例: 動画プレーヤー → カラオケ遅延 の順に依存）。
3. **【配置方針の明文化】独自機能の置き場所が未規定。** §5 の `com.xenuyu.*` 方針を採用するか、`Basis/Assets` に置くかをプロジェクトとして確定する。決めないとアップストリーム（Basis 本体）の追従が困難になる。
4. **【ロゴ画像が巨大】`1782709126879.png` が約 2MB。** README 用ロゴとしては大きすぎ、リポジトリを重くする。リサイズ（160x160 表示なのに原寸が大）または Releases/外部ホストへ。ファイル名も意味のある名前へリネーム推奨。
5. **【アップストリーム追従戦略が不明】** フォーク元 Basis の更新を取り込む手順（remote 設定・マージ方針・改変箇所の管理）が文書化されていない。`com.basis.*` を直接いじるほど追従が苦しくなるため、§5 と合わせて方針を決める。
6. **【ライセンス/サードパーティ確認】** 動画プレーヤー(ytdlp)・mediapipe・SteamVR・VRM 変換など、配布物に影響する依存が多い。実装前に各依存の配布ライセンスを LICENSE / TRADEMARK と突き合わせる。
7. **【テスト基盤】** STYLE.md は手動テスト前提。自動テスト（EditMode/PlayMode）や CI でのスモークが薄いなら、回帰しやすい「Desktop↔VR 切替・avatar 差替・server 参加/離脱」だけでも最小の自動チェックを検討。
8. **【日本語 README の英語版欠如】** ドキュメントが日本語化された（直近コミット）一方で、OSS としてのコントリビューター向け英語版が無い。最低限 README に英語サマリ or `README.en.md` を検討（任意）。

---

## 8. 作業ログ（随時追記）

各機能の進捗をフェーズ単位で残す。フォーマット例:

```
### <機能名>  — 状態: フェーズN
- 決定事項: ...
- 影響パッケージ: ...
- 未検証: ...（Editor/Player 確認待ち 等）
```

（ここから追記）
