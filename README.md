# unimaxstavatar

unimaxstavatar package is for developing Unity apps that target native platforms.

## 개요

아바타시스템은 메타버스 구현을 위한 핵심요소 중 하나인 아바타 및 착장 아이템을 개방성이 보장된 형태로 지속가능하고, 확장 가능한 생태계로 발전시키는 것을 지향합니다.

이에 특정 기업 혹은 주체가 모든 디지털 콘텐츠를 독점하는 것이 아닌 외부의 개발자, 제작사, 혹은 크리에이터가 자유롭게 참여할 수 있도록 데이터를 개방하여 프로슈머 생태계를 혁신하고 다양한 참여자들과 협업 및 발전시킬 수 있는 플랫폼을 조성하고자 합니다.

따라서, MAXVERSE 아바타시스템에서는 다양한 아바타 및 아이템들을 제작하여 등록할 수 있습니다. 서비스 개발사 및 제작사는 시스템에 등록된 다양한 아바타와 아이템들을 활용할 수 있는 아바타 생성 및 편집 페이지소스코드를 활용하여 각자의 서비스 개발 방향에 맞게 활용 할 수 있습니다.

개발사 및 제작사는 MAXST에서 자체 제작한 아바타 모델과 아이템들은 물론 다양한 참여자들이 함께 만들어 나가는 아바타 시스템을 활용하여 손쉽게 사용자들에게 아바타 기반의 서비스를 제공할 수 있습니다. 서비스 사용자는 지속적으로 확장되어 나가는 아바타 시스템의 혜택을 통해 다양한 아바타 및 아이템들을 활용하여 더 나은 서비스 경험과 편의성을 느낄 수 있습니다.

자세한 내용은 [maxverse.io](https://doc.maxverse.io/avatar) 에서 확인 가능합니다.

## 아바타 리소스 제작 및 업로드  

[기본으로 제공되는 리소스](https://doc.maxverse.io/avatar-getting-started) 외에 직접 재작한  아바타리소스를 앱에서 이용하고 싶다면, [제작 가이드1](https://doc.maxverse.io/avatar-modeling), [제작 가이드2](https://doc.maxverse.io/avatar-create-item) 를 참고하여 리소스를 준비해야 합니다.  

준비된 아바타리소스는 [Unity Addressables](https://docs.unity3d.com/Manual/com.unity.addressables.html), [아이템 빌드](https://doc.maxverse.io/avatar-build-item) 를 참고하여 리소스가 주소를 통해 호출될수있도록 변경되어야 합니다.  

변경된 데이터는 [아이템 업로드](https://doc.maxverse.io/avatar-upload-item) 내용을 참고하여 맥스버스 콘솔을 통하여 업로드를 진행합니다. 

### Unity Version
* [Unity 2021.3.16f1](https://unity.com/releases/editor/whats-new/2021.3.16)

### Platform
* Android
* iOS
* Window

## Docs about unimaxstavatar
You can check docs and guides at [avatar-apply](https://doc.maxverse.io/avatar-apply)

## License
The source code for the site is licensed under the MIT license, which you can find in the [LICENSE](https://github.com/maxstdev/unimaxstavatar/blob/main/LICENSE) file.

### Third Party
* [UniRx](https://github.com/neuecc/UniRx.git) : [MIT license](https://github.com/neuecc/UniRx/blob/master/LICENSE)
* [UniTask](https://github.com/Cysharp/UniTask.git) : [MIT license](https://github.com/Cysharp/UniTask/blob/master/LICENSE)
* [UMA](https://github.com/umasteeringgroup/UMA.git) : [MIT license](https://github.com/umasteeringgroup/UMA/blob/master/LICENSE).
